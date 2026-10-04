import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import {
  AssignFakturDto,
  CreateTaxEntryDto,
  CreateWhtCertificateDto,
  QueryTaxEntriesDto,
  SetTaxEntryStatusDto,
} from './dto/tax-subledger.dto';
import { syncTaxEntries } from './tax-subledger.sync';

const PPH_TYPES = ['PPH_22', 'PPH_23'] as const;

/**
 * A4 — Pajak pengadaan: PPN subledger per transaction (synced from posted
 * invoices), faktur pajak numbers + Coretax CSV export, PPh 22/23 withheld
 * by school bendahara with bukti potong upload/status and reconciliation
 * (expected vs certificates received), and a monthly tax report.
 */
@Injectable()
export class ErpTaxSubledgerService {
  constructor(private readonly prisma: PrismaService) {}

  private async fiscalPeriodFor(date: Date) {
    const p = await this.prisma.erpFiscalPeriod.findFirst({
      where: { startDate: { lte: date }, endDate: { gte: date }, deletedAt: null },
    });
    if (!p) throw new BadRequestException('Periode fiskal untuk tanggal tersebut tidak ditemukan.');
    return p;
  }

  private async idrCurrencyId() {
    const c = await this.prisma.erpCurrency.findFirst({ where: { code: 'IDR' } });
    if (!c) throw new BadRequestException('Mata uang IDR tidak ditemukan.');
    return c.id;
  }

  private monthRange(year: number, month: number) {
    return {
      gte: new Date(Date.UTC(year, month - 1, 1)),
      lt: new Date(Date.UTC(year, month, 1)),
    };
  }

  async sync(actorId?: string) {
    const r = await syncTaxEntries(this.prisma, actorId ? BigInt(actorId) : undefined);
    return { success: true, data: r };
  }

  async findEntries(query: QueryTaxEntriesDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 25;
    const where: Prisma.ErpFinTaxEntryWhereInput = { deletedAt: null };
    if (query.taxEntryType) where.taxEntryType = query.taxEntryType;
    if (query.status) where.status = query.status as any;
    if (query.year != null) {
      where.transactionDate = this.monthRange(query.year, query.month ?? 1);
      if (query.month == null) {
        where.transactionDate = {
          gte: new Date(Date.UTC(query.year, 0, 1)),
          lt: new Date(Date.UTC(query.year + 1, 0, 1)),
        };
      }
    }
    if (query.search?.trim()) {
      const q = query.search.trim();
      where.OR = [
        { docNumber: { contains: q, mode: 'insensitive' } },
        { partnerName: { contains: q, mode: 'insensitive' } },
        { fakturNumber: { contains: q, mode: 'insensitive' } },
      ];
    }
    const [items, total] = await this.prisma.$transaction([
      this.prisma.erpFinTaxEntry.findMany({
        where,
        orderBy: [{ transactionDate: 'desc' }, { id: 'desc' }],
        skip: (page - 1) * limit,
        take: limit,
      }),
      this.prisma.erpFinTaxEntry.count({ where }),
    ]);
    const taxIds = [...new Set(items.map((i) => i.taxId))];
    const taxes = taxIds.length
      ? await this.prisma.erpTax.findMany({
          where: { id: { in: taxIds } },
          select: { id: true, code: true, name: true },
        })
      : [];
    const taxById = new Map(taxes.map((t) => [t.id.toString(), t]));
    return {
      success: true,
      data: items.map((i) => ({ ...i, tax: taxById.get(i.taxId.toString()) ?? null })),
      meta: { page, limit, total, totalPages: Math.ceil(total / limit) || 1 },
    };
  }

  async createEntry(dto: CreateTaxEntryDto, actorId?: string) {
    const date = new Date(dto.transactionDate);
    const period = await this.fiscalPeriodFor(date);
    const tax = await this.prisma.erpTax.findUnique({ where: { id: BigInt(dto.taxId) } });
    if (!tax) throw new BadRequestException('Master pajak tidak ditemukan.');
    const partner = dto.partnerId
      ? await this.prisma.erpPartner.findUnique({
          where: { id: BigInt(dto.partnerId) },
          select: { name: true, taxNumber: true },
        })
      : null;
    const row = await this.prisma.erpFinTaxEntry.create({
      data: {
        module: dto.module ?? 'FIN',
        sourceDocType: dto.sourceDocType ?? null,
        sourceId: dto.sourceId ? BigInt(dto.sourceId) : null,
        docNumber: dto.docNumber,
        transactionDate: date,
        fiscalPeriodId: period.id,
        partnerId: dto.partnerId ? BigInt(dto.partnerId) : null,
        partnerNpwp: partner?.taxNumber ?? null,
        partnerName: partner?.name ?? null,
        taxId: tax.id,
        taxEntryType: dto.taxEntryType as any,
        dpp: dto.dpp,
        taxRate: tax.rate.toString(),
        taxAmount: dto.taxAmount,
        fakturNumber: dto.fakturNumber ?? null,
        fakturDate: dto.fakturDate ? new Date(dto.fakturDate) : null,
        status: 'DRAFT',
        currencyId: await this.idrCurrencyId(),
        exchangeRate: '1',
        createdById: actorId ? BigInt(actorId) : null,
      },
    });
    return { success: true, data: row };
  }

  async assignFaktur(id: bigint, dto: AssignFakturDto) {
    const entry = await this.prisma.erpFinTaxEntry.findFirst({ where: { id, deletedAt: null } });
    if (!entry) throw new NotFoundException('Entri pajak tidak ditemukan.');
    const fakturDate = dto.fakturDate ? new Date(dto.fakturDate) : entry.transactionDate;
    const updated = await this.prisma.erpFinTaxEntry.update({
      where: { id },
      data: { fakturNumber: dto.fakturNumber, fakturDate },
    });
    // Keep the source invoice's faktur number in step with the subledger.
    if (entry.sourceId && entry.sourceDocType === 'sls_invoices') {
      await this.prisma.erpSlsInvoice.updateMany({
        where: { id: entry.sourceId },
        data: { taxInvoiceNo: dto.fakturNumber },
      });
    }
    if (entry.sourceId && entry.sourceDocType === 'pur_invoices') {
      await this.prisma.erpPurInvoice.updateMany({
        where: { id: entry.sourceId },
        data: { taxInvoiceNo: dto.fakturNumber },
      });
    }
    return { success: true, data: updated };
  }

  async setStatus(id: bigint, dto: SetTaxEntryStatusDto) {
    const entry = await this.prisma.erpFinTaxEntry.findFirst({ where: { id, deletedAt: null } });
    if (!entry) throw new NotFoundException('Entri pajak tidak ditemukan.');
    if (entry.status === 'REPORTED' && dto.status === 'CANCELLED') {
      throw new BadRequestException('Entri yang sudah dilaporkan (REPORTED) tidak dapat dibatalkan.');
    }
    const data: Prisma.ErpFinTaxEntryUpdateInput = { status: dto.status as any };
    if (dto.status === 'REPORTED') {
      data.reportedAt = new Date();
      data.reportedPeriodId = entry.fiscalPeriodId;
    }
    const updated = await this.prisma.erpFinTaxEntry.update({ where: { id }, data });
    return { success: true, data: updated };
  }

  async coretaxCsv(year: number, month: number): Promise<{ csv: string; fileName: string }> {
    const rows = await this.prisma.erpFinTaxEntry.findMany({
      where: {
        taxEntryType: 'PPN_KELUARAN',
        status: { not: 'CANCELLED' },
        deletedAt: null,
        transactionDate: this.monthRange(year, month),
      },
      orderBy: { transactionDate: 'asc' },
    });
    const lines = [
      'NomorFaktur;TanggalFaktur;NPWP Pembeli;Nama Pembeli;DPP;PPN;Status',
    ];
    for (const r of rows) {
      lines.push(
        [
          r.fakturNumber ?? '',
          r.fakturDate ? r.fakturDate.toISOString().slice(0, 10) : r.transactionDate.toISOString().slice(0, 10),
          r.partnerNpwp ?? '',
          (r.partnerName ?? '').replace(/;/g, ','),
          r.dpp.toString(),
          r.taxAmount.toString(),
          r.status,
        ].join(';'),
      );
    }
    return {
      csv: lines.join('\r\n'),
      fileName: `coretax-ppn-keluaran-${year}-${String(month).padStart(2, '0')}.csv`,
    };
  }

  // ── Bukti potong (PPh 22/23) ───────────────────────────────────────────────

  async withholding() {
    const entries = await this.prisma.erpFinTaxEntry.findMany({
      where: {
        taxEntryType: { in: [...PPH_TYPES] },
        status: { not: 'CANCELLED' },
        deletedAt: null,
      },
      orderBy: { transactionDate: 'desc' },
      include: { certificates: { where: { deletedAt: null } } },
    });
    const data = entries.map((e) => {
      const certified = e.certificates
        .filter((c) => c.status === 'ISSUED')
        .reduce((s, c) => s + Number(c.amountWithheld.toString()), 0);
      const expected = Number(e.taxAmount.toString());
      const diff = expected - certified;
      return {
        ...e,
        certifiedTotal: String(certified),
        diff: String(diff),
        reconcileStatus: diff === 0 ? 'LENGKAP' : certified === 0 ? 'BELUM_ADA_BUKTI' : diff > 0 ? 'KURANG' : 'LEBIH',
      };
    });
    return { success: true, data };
  }

  async listCertificates() {
    const certs = await this.prisma.erpFinWhtCertificate.findMany({
      where: { deletedAt: null },
      orderBy: { id: 'desc' },
      take: 200,
    });
    return { success: true, data: certs };
  }

  async createCertificate(dto: CreateWhtCertificateDto, actorId?: string) {
    const partner = await this.prisma.erpPartner.findUnique({
      where: { id: BigInt(dto.partnerId) },
      select: { name: true, taxNumber: true },
    });
    if (!partner) throw new BadRequestException('Partner tidak ditemukan.');
    const date = new Date(dto.transactionDate);
    const period = await this.fiscalPeriodFor(date);
    const dup = await this.prisma.erpFinWhtCertificate.findFirst({
      where: { certNumber: dto.certNumber },
    });
    if (dup) throw new BadRequestException(`Nomor bukti potong ${dto.certNumber} sudah tercatat.`);
    const row = await this.prisma.erpFinWhtCertificate.create({
      data: {
        certNumber: dto.certNumber,
        pphType: dto.pphType as any,
        transactionDate: date,
        fiscalPeriodId: period.id,
        partnerId: BigInt(dto.partnerId),
        partnerNpwp: partner.taxNumber ?? null,
        partnerName: partner.name,
        dpp: dto.dpp,
        rate: dto.rate,
        amountWithheld: dto.amountWithheld,
        status: 'ISSUED',
        taxEntryId: dto.taxEntryId ? BigInt(dto.taxEntryId) : null,
        sourceDocType: dto.taxEntryId ? 'fin_tax_entries' : null,
        sourceId: dto.taxEntryId ? BigInt(dto.taxEntryId) : null,
        notes: dto.notes ?? null,
        createdById: actorId ? BigInt(actorId) : null,
      },
    });
    return { success: true, data: row };
  }

  async cancelCertificate(id: bigint) {
    const cert = await this.prisma.erpFinWhtCertificate.findFirst({
      where: { id, deletedAt: null },
    });
    if (!cert) throw new NotFoundException('Bukti potong tidak ditemukan.');
    const updated = await this.prisma.erpFinWhtCertificate.update({
      where: { id },
      data: { status: 'CANCELLED' },
    });
    return { success: true, data: updated };
  }

  // ── Laporan bulanan ────────────────────────────────────────────────────────

  async monthlyReport(year: number, month: number) {
    const range = this.monthRange(year, month);
    const entries = await this.prisma.erpFinTaxEntry.findMany({
      where: { deletedAt: null, status: { not: 'CANCELLED' }, transactionDate: range },
    });
    const certs = await this.prisma.erpFinWhtCertificate.findMany({
      where: { deletedAt: null, status: 'ISSUED', transactionDate: range },
    });
    const sum = (list: typeof entries, type: string) => {
      const rows = list.filter((e) => e.taxEntryType === type);
      return {
        count: rows.length,
        dpp: rows.reduce((s, e) => s + Number(e.dpp.toString()), 0),
        tax: rows.reduce((s, e) => s + Number(e.taxAmount.toString()), 0),
      };
    };
    const certSum = (type: string) =>
      certs
        .filter((c) => c.pphType === type)
        .reduce((s, c) => s + Number(c.amountWithheld.toString()), 0);
    const ppnKeluaran = sum(entries, 'PPN_KELUARAN');
    const ppnMasukan = sum(entries, 'PPN_MASUKAN');
    const pph22 = sum(entries, 'PPH_22');
    const pph23 = sum(entries, 'PPH_23');
    return {
      success: true,
      data: {
        year,
        month,
        ppnKeluaran,
        ppnMasukan,
        ppnNetto: ppnKeluaran.tax - ppnMasukan.tax,
        pph22: { ...pph22, certified: certSum('PPH_22') },
        pph23: { ...pph23, certified: certSum('PPH_23') },
        reportedCount: entries.filter((e) => e.status === 'REPORTED').length,
        draftCount: entries.filter((e) => e.status === 'DRAFT').length,
      },
    };
  }

  async monthlyCsv(year: number, month: number): Promise<{ csv: string; fileName: string }> {
    const { data } = await this.monthlyReport(year, month);
    const lines = [
      `Laporan Pajak Bulanan;${year}-${String(month).padStart(2, '0')}`,
      'Jenis;Jumlah Entri;DPP;Pajak;Bukti Potong Diterima',
      `PPN Keluaran;${data.ppnKeluaran.count};${data.ppnKeluaran.dpp};${data.ppnKeluaran.tax};`,
      `PPN Masukan;${data.ppnMasukan.count};${data.ppnMasukan.dpp};${data.ppnMasukan.tax};`,
      `PPN Netto (Kurang/Lebih Bayar);;;${data.ppnNetto};`,
      `PPh 22 Dipotong Bendahara;${data.pph22.count};${data.pph22.dpp};${data.pph22.tax};${data.pph22.certified}`,
      `PPh 23 Dipotong Bendahara;${data.pph23.count};${data.pph23.dpp};${data.pph23.tax};${data.pph23.certified}`,
      `Entri DRAFT;${data.draftCount};;;`,
      `Entri REPORTED;${data.reportedCount};;;`,
    ];
    return { csv: lines.join('\r\n'), fileName: `laporan-pajak-${year}-${String(month).padStart(2, '0')}.csv` };
  }
}
