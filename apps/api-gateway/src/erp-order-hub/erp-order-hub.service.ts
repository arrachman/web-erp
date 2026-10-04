import { Injectable, NotFoundException } from '@nestjs/common';
import { ErpOrderHubStatus, Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { QueryOrderHubDto } from './dto/query-order-hub.dto';
import { deriveHubStage, HubFacts } from './order-hub.stage';

type OrderRow = Prisma.ErpSlsOrderGetPayload<{ include: { lines: true } }>;

interface StageInfo {
  stage: ErpOrderHubStatus;
  facts: HubFacts;
  counts: { deliveryOrders: number; deliveryReports: number; invoices: number };
}

/**
 * A2 Order Hub — one queue over all sales orders, regardless of channel.
 * The unified stage is DERIVED from the document chain (see order-hub.stage)
 * and every observed transition is appended to `sls_order_status_logs`
 * (source='DERIVED'), which doubles as the status-change audit trail.
 */
@Injectable()
export class ErpOrderHubService {
  constructor(private readonly prisma: PrismaService) {}

  // ── Stage computation ──────────────────────────────────────────────────────

  private async computeStages(orderIds: bigint[]): Promise<Map<string, StageInfo>> {
    const result = new Map<string, StageInfo>();
    if (!orderIds.length) return result;

    const orders = await this.prisma.erpSlsOrder.findMany({
      where: { id: { in: orderIds } },
      select: { id: true, status: true },
    });
    const dos = await this.prisma.erpSlsDeliveryOrder.findMany({
      where: { orderId: { in: orderIds }, deletedAt: null },
      select: { id: true, orderId: true, postingStatus: true },
    });
    const doIds = dos.map((d) => d.id);
    const drs = doIds.length
      ? await this.prisma.erpSlsDeliveryReport.findMany({
          where: { deliveryOrderId: { in: doIds }, deletedAt: null },
          select: { deliveryOrderId: true, status: true, acceptedAt: true },
        })
      : [];
    const invoices = await this.prisma.erpSlsInvoice.findMany({
      where: { orderId: { in: orderIds }, deletedAt: null },
      select: { orderId: true, postingStatus: true, settlementStatus: true },
    });

    const doByOrder = new Map<string, typeof dos>();
    for (const d of dos) {
      const k = String(d.orderId);
      doByOrder.set(k, [...(doByOrder.get(k) ?? []), d]);
    }
    const postedDoIds = new Set(dos.filter((d) => d.postingStatus === 'POSTED').map((d) => String(d.id)));
    const drCountByOrder = new Map<string, number>();
    let postedDrOrderIds = new Set<string>();
    const doOrderById = new Map(dos.map((d) => [String(d.id), String(d.orderId)]));
    for (const dr of drs) {
      const orderId = doOrderById.get(String(dr.deliveryOrderId));
      if (!orderId) continue;
      drCountByOrder.set(orderId, (drCountByOrder.get(orderId) ?? 0) + 1);
      if (dr.acceptedAt) postedDrOrderIds = postedDrOrderIds.add(orderId);
    }
    const invByOrder = new Map<string, typeof invoices>();
    for (const inv of invoices) {
      const k = String(inv.orderId);
      invByOrder.set(k, [...(invByOrder.get(k) ?? []), inv]);
    }

    for (const o of orders) {
      const k = o.id.toString();
      const myDos = doByOrder.get(k) ?? [];
      const myInvoices = invByOrder.get(k) ?? [];
      const facts: HubFacts = {
        orderStatus: o.status,
        hasDeliveryOrder: myDos.length > 0,
        hasPostedDeliveryOrder: myDos.some((d) => postedDoIds.has(String(d.id))),
        hasAcceptedDeliveryReport: postedDrOrderIds.has(k),
        hasInvoice: myInvoices.some((i) => i.postingStatus === 'POSTED'),
        hasPaidInvoice: myInvoices.some((i) => i.settlementStatus === 'PAID'),
      };
      result.set(k, {
        stage: deriveHubStage(facts),
        facts,
        counts: {
          deliveryOrders: myDos.length,
          deliveryReports: drCountByOrder.get(k) ?? 0,
          invoices: myInvoices.length,
        },
      });
    }
    return result;
  }

  /** Append a status-log row for every order whose derived stage moved. */
  private async logTransitions(
    stages: Map<string, StageInfo>,
    actorId?: string,
  ): Promise<number> {
    const ids = [...stages.keys()].map((k) => BigInt(k));
    if (!ids.length) return 0;
    const logs = await this.prisma.erpSlsOrderStatusLog.findMany({
      where: { orderId: { in: ids } },
      orderBy: [{ orderId: 'asc' }, { id: 'desc' }],
      select: { orderId: true, hubStatus: true },
    });
    const lastByOrder = new Map<string, ErpOrderHubStatus>();
    for (const l of logs) {
      const k = l.orderId.toString();
      if (!lastByOrder.has(k)) lastByOrder.set(k, l.hubStatus);
    }
    const rows: Prisma.ErpSlsOrderStatusLogCreateManyInput[] = [];
    for (const [k, info] of stages) {
      if (lastByOrder.get(k) !== info.stage) {
        rows.push({
          orderId: BigInt(k),
          hubStatus: info.stage,
          source: 'DERIVED',
          createdById: actorId ? BigInt(actorId) : null,
        });
      }
    }
    if (rows.length) {
      await this.prisma.erpSlsOrderStatusLog.createMany({ data: rows });
    }
    return rows.length;
  }

  // ── Queries ────────────────────────────────────────────────────────────────

  private buildWhere(query: QueryOrderHubDto): Prisma.ErpSlsOrderWhereInput {
    const where: Prisma.ErpSlsOrderWhereInput = { deletedAt: null };
    if (query.channel) where.channel = query.channel as never;
    if (query.fundingSource) where.fundingSource = query.fundingSource as never;
    if (query.budgetYear != null) where.budgetYear = query.budgetYear;
    if (query.customerId) where.customerId = BigInt(query.customerId);
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { docNumber: { contains: q, mode: 'insensitive' } },
        { externalOrderId: { contains: q, mode: 'insensitive' } },
        { code: { contains: q, mode: 'insensitive' } },
      ];
    }
    return where;
  }

  private async enrich(items: OrderRow[], stages: Map<string, StageInfo>) {
    const customerIds = [...new Set(items.map((i) => i.customerId).filter((v): v is bigint => v != null))];
    const customers = customerIds.length
      ? await this.prisma.erpPartner.findMany({
          where: { id: { in: customerIds } },
          select: { id: true, code: true, name: true },
        })
      : [];
    const byId = new Map(customers.map((c) => [c.id.toString(), c]));
    return items.map((o) => {
      const info = stages.get(o.id.toString());
      return {
        ...o,
        hubStage: info?.stage ?? null,
        hubCounts: info?.counts ?? { deliveryOrders: 0, deliveryReports: 0, invoices: 0 },
        customer: o.customerId ? byId.get(o.customerId.toString()) ?? null : null,
      };
    });
  }

  async findAll(query: QueryOrderHubDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 25;
    const sortBy = query.sortBy ?? 'docNumber';
    const sortDir = query.sortDir ?? 'desc';
    const where = this.buildWhere(query);

    // Stage is derived, so a stage filter must be applied after computing it:
    // evaluate all matching orders (queue-sized data), then paginate.
    if (query.hubStatus) {
      const all = await this.prisma.erpSlsOrder.findMany({
        where,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
      });
      const stages = await this.computeStages(all.map((o) => o.id));
      await this.logTransitions(stages);
      const filtered = all.filter((o) => stages.get(o.id.toString())?.stage === query.hubStatus);
      const total = filtered.length;
      const pageItems = filtered.slice((page - 1) * limit, page * limit);
      return {
        success: true,
        data: await this.enrich(pageItems, stages),
        meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
      };
    }

    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpSlsOrder.findMany({
        where,
        orderBy: [{ [sortBy]: sortDir }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
        include: { lines: { orderBy: { lineNo: 'asc' } } },
      }),
      this.prisma.erpSlsOrder.count({ where }),
    ]);
    const stages = await this.computeStages(items.map((o) => o.id));
    await this.logTransitions(stages);
    return {
      success: true,
      data: await this.enrich(items, stages),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async findOne(orderId: bigint) {
    const order = await this.prisma.erpSlsOrder.findFirst({
      where: { id: orderId, deletedAt: null },
      include: { lines: { orderBy: { lineNo: 'asc' } } },
    });
    if (!order) throw new NotFoundException('Sales order tidak ditemukan.');
    const stages = await this.computeStages([orderId]);
    await this.logTransitions(stages);
    const info = stages.get(orderId.toString());

    const dos = await this.prisma.erpSlsDeliveryOrder.findMany({
      where: { orderId, deletedAt: null },
      orderBy: { docDate: 'asc' },
      select: { id: true, docNumber: true, docDate: true, status: true, postingStatus: true },
    });
    const drs = dos.length
      ? await this.prisma.erpSlsDeliveryReport.findMany({
          where: { deliveryOrderId: { in: dos.map((d) => d.id) }, deletedAt: null },
          orderBy: { docDate: 'asc' },
          select: { id: true, docNumber: true, docDate: true, status: true, deliveryOrderId: true, acceptedAt: true, acceptedByName: true },
        })
      : [];
    const invoices = await this.prisma.erpSlsInvoice.findMany({
      where: { orderId, deletedAt: null },
      orderBy: { docDate: 'asc' },
      select: { id: true, docNumber: true, docDate: true, status: true, settlementStatus: true, grandTotal: true },
    });
    const logs = await this.prisma.erpSlsOrderStatusLog.findMany({
      where: { orderId },
      orderBy: { id: 'asc' },
    });
    const [enriched] = await this.enrich([order], stages);
    return {
      success: true,
      data: {
        ...enriched,
        hubStage: info?.stage ?? null,
        chain: { deliveryOrders: dos, deliveryReports: drs, invoices },
        statusLogs: logs,
      },
    };
  }

  /** Recompute stages for every live order and log any transition. */
  async sync(actorId?: string) {
    const orders = await this.prisma.erpSlsOrder.findMany({
      where: { deletedAt: null },
      select: { id: true },
    });
    let updated = 0;
    const chunk = 200;
    for (let i = 0; i < orders.length; i += chunk) {
      const stages = await this.computeStages(orders.slice(i, i + chunk).map((o) => o.id));
      updated += await this.logTransitions(stages, actorId);
    }
    return { success: true, data: { checked: orders.length, transitionsLogged: updated } };
  }
}
