import {
  BadRequestException,
  Injectable,
  NotFoundException,
} from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { ErpSlsOrdersService } from '../erp-sls-orders/erp-sls-orders.service';
import { ErpContractsService } from '../erp-contracts/erp-contracts.service';
import { ErpBundlesService } from '../erp-contracts/erp-bundles.service';
import {
  deriveHubStage,
  HUB_STAGE_LABELS,
  HubFacts,
} from '../erp-order-hub/order-hub.stage';
import { PortalCreateOrderDto } from './dto/erp-portal.dto';

const id = (v: bigint | null | undefined) => (v == null ? null : v.toString());
const money = (v: any) => (v == null ? '0' : v.toString());

function channelVisible(channels: unknown): boolean {
  if (!Array.isArray(channels) || channels.length === 0) return true;
  return channels.some((c) =>
    ['PORTAL_SEKOLAH', 'PORTAL', 'SEMUA', 'ALL'].includes(String(c).toUpperCase()),
  );
}

/**
 * Fase 3 W2 — sisi belanja portal: katalog dari D1 (profil kanal), order masuk
 * Order Hub sebagai kanal PORTAL_SEKOLAH lewat service SO yang sama dengan
 * admin, status diturunkan dari rantai dokumen (deriveHubStage A2), tagihan
 * dibaca dari invoice customer yang sama. Tidak ada tabel order duplikat.
 */
@Injectable()
export class ErpPortalShopService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly orders: ErpSlsOrdersService,
    private readonly contracts: ErpContractsService,
    private readonly bundles: ErpBundlesService,
  ) {}

  // ── Catalog ───────────────────────────────────────────────────────────────

  async catalog(opts: { search?: string; jenjang?: string; page?: number; pageSize?: number }, account?: any) {
    const page = Math.max(1, opts.page ?? 1);
    const pageSize = Math.min(100, Math.max(1, opts.pageSize ?? 24));
    const items = await this.prisma.erpItem.findMany({
      where: {
        deletedAt: null,
        salePrice: { gt: 0 },
        ...(opts.search
          ? {
              OR: [
                { name: { contains: opts.search, mode: 'insensitive' } },
                { code: { contains: opts.search, mode: 'insensitive' } },
              ],
            }
          : {}),
      },
      orderBy: { name: 'asc' },
    });
    const itemIds = items.map((i) => i.id);
    const profiles = await this.prisma.erpItemCatalogProfile.findMany({
      where: { itemId: { in: itemIds } },
    });
    const profileByItem = new Map(profiles.map((p) => [p.itemId.toString(), p]));
    const categoryIds = [...new Set(items.map((i) => i.categoryId).filter(Boolean))] as bigint[];
    const unitIds = [...new Set(items.map((i) => i.baseUnitId).filter(Boolean))] as bigint[];
    const categories = categoryIds.length
      ? await this.prisma.erpItemCategory.findMany({ where: { id: { in: categoryIds } } })
      : [];
    const units = unitIds.length
      ? await this.prisma.erpUnit.findMany({ where: { id: { in: unitIds } } })
      : [];
    const catName = new Map(categories.map((c) => [c.id.toString(), c.name]));
    const unitName = new Map(units.map((u) => [u.id.toString(), u.name]));

    let rows = items
      .map((i) => {
        const p = profileByItem.get(i.id.toString());
        return { item: i, profile: p };
      })
      .filter(({ profile }) => !profile || (profile.isActive && channelVisible(profile.channels)))
      .filter(({ profile }) => !opts.jenjang || profile?.jenjang === opts.jenjang)
      .map(({ item: i, profile: p }) => ({
        id: id(i.id),
        code: i.code,
        name: i.name,
        salePrice: money(i.salePrice),
        hetPrice: p?.hetPrice != null ? money(p.hetPrice) : null,
        publisherName: p?.publisherName ?? null,
        jenjang: p?.jenjang ?? null,
        gradeLevel: p?.gradeLevel ?? null,
        subject: p?.subject ?? null,
        isCustomPrint: p?.isCustomPrint ?? false,
        category: i.categoryId ? catName.get(i.categoryId.toString()) ?? null : null,
        unit: unitName.get(i.baseUnitId.toString()) ?? null,
        price: money(i.salePrice),
        priceSource: 'STANDAR' as string,
        bundle: null as null | { components: { itemId: string; name: string | null; quantity: string; unit: string | null }[] },
      }));
    const total = rows.length;
    rows = rows.slice((page - 1) * pageSize, page * pageSize);
    // W7: harga yang tampil = harga kontrak sekolah akun (berlapis) bila akun
    // tertaut partner; pengunjung publik (sorotan landing) melihat standar.
    // Item paket (bundle) menampilkan isi paketnya.
    if (rows.length) {
      const pageItems = rows
        .map((r) => items.find((i) => i.id.toString() === r.id))
        .filter(Boolean) as typeof items;
      const bundleMap = await this.bundles.bundlesForItems(pageItems.map((i) => i.id));
      const partnerId: bigint | null = account?.partnerId ?? null;
      const priceMap = partnerId
        ? await this.contracts.resolveMany(
            partnerId,
            pageItems.map((i) => ({ id: i.id, categoryId: i.categoryId, salePrice: i.salePrice })),
          )
        : null;
      for (const row of rows) {
        const key = row.id as string;
        const resolved = priceMap?.get(key);
        if (resolved) {
          row.price = resolved.price;
          row.priceSource = resolved.source;
        }
        const bundle = bundleMap.get(key);
        if (bundle) {
          row.bundle = {
            components: bundle.components.map((c) => ({
              itemId: c.itemId,
              name: c.name,
              quantity: c.quantity,
              unit: c.unit,
            })),
          };
        }
      }
    }
    return { data: rows, total, page, pageSize };
  }

  async highlights() {
    const { data } = await this.catalog({ page: 1, pageSize: 8 });
    return { data };
  }

  // ── Orders ────────────────────────────────────────────────────────────────

  private async idrId(): Promise<bigint> {
    const c = await this.prisma.erpCurrency.findFirst({ where: { code: 'IDR' } });
    if (!c) throw new BadRequestException('Mata uang IDR tidak ditemukan');
    return c.id;
  }

  async createOrder(account: any, dto: PortalCreateOrderDto) {
    const partnerId: bigint = account.partnerId;
    // W4: kanal mengikuti peran akun — orang tua memakai PORTAL_ORANGTUA.
    const isParent = account.role === 'ORANG_TUA';
    const channel = isParent ? 'PORTAL_ORANGTUA' : 'PORTAL_SEKOLAH';
    const externalOrderId = dto.clientRef
      ? `PORTAL-${account.id}-${dto.clientRef}`
      : `PORTAL-${account.id}-${Date.now()}`;
    const existing = await this.prisma.erpSlsOrder.findFirst({
      where: { channel: channel as any, externalOrderId, deletedAt: null },
    });
    if (existing) return this.orderView(existing, await this.factsFor([existing]));

    const itemIds = dto.lines.map((l) => BigInt(l.itemId));
    const items = await this.prisma.erpItem.findMany({
      where: { id: { in: itemIds }, deletedAt: null },
    });
    const byId = new Map(items.map((i) => [i.id.toString(), i]));
    // W7: harga baris = harga kontrak sekolah (berlapis), bukan harga standar.
    const priceMap = await this.contracts.resolveMany(
      partnerId,
      items.map((i) => ({ id: i.id, categoryId: i.categoryId, salePrice: i.salePrice })),
    );
    const lines = dto.lines.map((l, idx) => {
      const item = byId.get(l.itemId);
      if (!item) throw new BadRequestException(`Item ${l.itemId} tidak ditemukan`);
      return {
        itemId: l.itemId,
        quantity: String(l.quantity),
        unitId: item.baseUnitId.toString(),
        unitPrice: priceMap.get(l.itemId)?.price ?? money(item.salePrice),
        lineNo: idx + 1,
      };
    });
    const branch = await this.prisma.erpBranch.findFirst({
      orderBy: { id: 'asc' },
      select: { id: true },
    });
    if (!branch) throw new BadRequestException('Cabang perusahaan belum diatur.');
    const meta = (account.metadata ?? {}) as Record<string, unknown>;
    const created: any = await this.orders.create(
      {
        docDate: new Date().toISOString().slice(0, 10),
        branchId: branch.id.toString(),
        customerId: partnerId.toString(),
        currencyId: (await this.idrId()).toString(),
        exchangeRate: '1',
        channel,
        externalOrderId,
        // Orang tua membayar sendiri (bukan dana BOS sekolah).
        fundingSource: isParent ? 'NON_BOS' : dto.fundingSource,
        budgetYear: dto.budgetYear,
        notes: dto.notes,
        ...(isParent
          ? {
              customFields: {
                portalParent: {
                  accountId: account.id.toString(),
                  studentName: (meta.studentName as string) ?? null,
                  studentClass: (meta.studentClass as string) ?? null,
                },
              },
            }
          : {}),
        lines,
      } as any,
      undefined,
    );
    const order = created?.data ?? created;
    const fresh = await this.prisma.erpSlsOrder.findUnique({ where: { id: BigInt(order.id) } });
    if (!fresh) throw new BadRequestException('Order gagal dibuat');
    return this.orderView(fresh, await this.factsFor([fresh]));
  }

  private async factsFor(orders: any[]): Promise<Map<string, HubFacts>> {
    const ids = orders.map((o) => o.id as bigint);
    const dos = ids.length
      ? await this.prisma.erpSlsDeliveryOrder.findMany({
          where: { orderId: { in: ids }, deletedAt: null },
          select: { id: true, orderId: true, status: true },
        })
      : [];
    const doIds = dos.map((d) => d.id);
    const reports = doIds.length
      ? await this.prisma.erpSlsDeliveryReport.findMany({
          where: { deliveryOrderId: { in: doIds }, acceptedAt: { not: null }, deletedAt: null },
          select: { deliveryOrderId: true },
        })
      : [];
    const invoices = ids.length
      ? await this.prisma.erpSlsInvoice.findMany({
          where: { orderId: { in: ids }, deletedAt: null },
          select: { orderId: true, settlementStatus: true },
        })
      : [];
    const acceptedDoIds = new Set(reports.map((r) => r.deliveryOrderId?.toString()));
    const map = new Map<string, HubFacts>();
    for (const o of orders) {
      const myDos = dos.filter((d) => d.orderId === o.id);
      const myInv = invoices.filter((v) => v.orderId === o.id);
      map.set(o.id.toString(), {
        orderStatus: o.status,
        hasDeliveryOrder: myDos.length > 0,
        hasPostedDeliveryOrder: myDos.some((d) => d.status === 'POSTED'),
        hasAcceptedDeliveryReport: myDos.some((d) => acceptedDoIds.has(d.id.toString())),
        hasInvoice: myInv.length > 0,
        hasPaidInvoice: myInv.some((v) => v.settlementStatus === 'PAID'),
      });
    }
    return map;
  }

  private orderView(o: any, facts: Map<string, HubFacts>) {
    const f = facts.get(o.id.toString());
    const stage = f ? deriveHubStage(f) : 'BARU';
    return {
      id: id(o.id),
      docNumber: o.docNumber,
      docDate: o.docDate,
      status: o.status,
      channel: o.channel,
      fundingSource: o.fundingSource ?? null,
      budgetYear: o.budgetYear ?? null,
      grandTotal: money(o.grandTotal),
      notes: o.notes ?? null,
      stage,
      stageLabel: (HUB_STAGE_LABELS as any)[stage] ?? stage,
      createdAt: o.createdAt,
    };
  }

  /** W4: orang tua hanya boleh melihat order miliknya sendiri di sekolah itu. */
  private isParent(account: any): boolean {
    return account?.role === 'ORANG_TUA';
  }

  private ownOrderScope(account: any): Record<string, unknown> {
    if (!this.isParent(account)) return {};
    return {
      channel: 'PORTAL_ORANGTUA' as any,
      externalOrderId: { startsWith: `PORTAL-${account.id}-` },
    };
  }

  async listOrders(account: any) {
    const orders = await this.prisma.erpSlsOrder.findMany({
      where: { customerId: account.partnerId, deletedAt: null, ...this.ownOrderScope(account) },
      orderBy: { docDate: 'desc' },
      take: 100,
    });
    const facts = await this.factsFor(orders);
    return { data: orders.map((o) => this.orderView(o, facts)), total: orders.length };
  }

  async orderDetail(account: any, orderId: string) {
    const order = await this.prisma.erpSlsOrder.findUnique({ where: { id: BigInt(orderId) } });
    if (!order || order.deletedAt || order.customerId !== account.partnerId) {
      throw new NotFoundException('Pesanan tidak ditemukan');
    }
    if (this.isParent(account)) {
      const prefix = `PORTAL-${account.id}-`;
      if ((order as any).channel !== 'PORTAL_ORANGTUA' || !order.externalOrderId?.startsWith(prefix)) {
        throw new NotFoundException('Pesanan tidak ditemukan');
      }
    }
    const lines = await this.prisma.erpSlsOrderLine.findMany({
      where: { orderId: order.id },
      orderBy: { lineNo: 'asc' },
    });
    const itemIds = lines.map((l) => l.itemId);
    const items = itemIds.length
      ? await this.prisma.erpItem.findMany({ where: { id: { in: itemIds } } })
      : [];
    const itemName = new Map(items.map((i) => [i.id.toString(), i.name]));
    const dos = await this.prisma.erpSlsDeliveryOrder.findMany({
      where: { orderId: order.id, deletedAt: null },
      orderBy: { docDate: 'asc' },
    });
    const invoices = await this.prisma.erpSlsInvoice.findMany({
      where: { orderId: order.id, deletedAt: null },
      orderBy: { docDate: 'asc' },
    });
    const facts = await this.factsFor([order]);
    return {
      ...this.orderView(order, facts),
      lines: lines.map((l: any) => ({
        id: id(l.id),
        itemId: id(l.itemId),
        itemName: itemName.get(l.itemId.toString()) ?? null,
        quantity: money(l.quantity),
        unitPrice: money(l.unitPrice),
        lineTotal: (Number(l.quantity) * Number(l.unitPrice)).toFixed(4),
      })),
      deliveryOrders: dos.map((d) => ({
        id: id(d.id),
        docNumber: d.docNumber,
        docDate: d.docDate,
        status: d.status,
      })),
      invoices: invoices.map((v) => ({
        id: id(v.id),
        docNumber: v.docNumber,
        docDate: v.docDate,
        dueDate: v.dueDate,
        grandTotal: money(v.grandTotal),
        settlementStatus: v.settlementStatus,
        status: v.status,
      })),
    };
  }

  async listInvoices(account: any) {
    // W4: tagihan orang tua = tagihan dari ordernya sendiri saja.
    let ownOrderIds: bigint[] | null = null;
    if (this.isParent(account)) {
      const own = await this.prisma.erpSlsOrder.findMany({
        where: { customerId: account.partnerId, deletedAt: null, ...this.ownOrderScope(account) },
        select: { id: true },
      });
      ownOrderIds = own.map((o) => o.id);
      if (!ownOrderIds.length) return { data: [], total: 0 };
    }
    const invoices = await this.prisma.erpSlsInvoice.findMany({
      where: {
        customerId: account.partnerId,
        deletedAt: null,
        ...(ownOrderIds ? { orderId: { in: ownOrderIds } } : {}),
      },
      orderBy: { docDate: 'desc' },
      take: 100,
    });
    const orderIds = [...new Set(invoices.map((v) => v.orderId).filter(Boolean))] as bigint[];
    const orders = orderIds.length
      ? await this.prisma.erpSlsOrder.findMany({ where: { id: { in: orderIds } } })
      : [];
    const orderNo = new Map(orders.map((o) => [o.id.toString(), o.docNumber]));
    return {
      data: invoices.map((v) => ({
        id: id(v.id),
        docNumber: v.docNumber,
        docDate: v.docDate,
        dueDate: v.dueDate,
        grandTotal: money(v.grandTotal),
        settlementStatus: v.settlementStatus,
        status: v.status,
        orderId: id(v.orderId),
        orderDocNumber: v.orderId ? orderNo.get(v.orderId.toString()) ?? null : null,
      })),
      total: invoices.length,
    };
  }
}
