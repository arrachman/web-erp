import { BadRequestException, Injectable, NotFoundException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import {
  ArriveTripStopDto,
  CreateDeliveryTripDto,
  CreateVehicleDto,
  FailTripStopDto,
  QueryDeliveryTripsDto,
  SetTripCostsDto,
  UpdateDeliveryTripDto,
  UpdateVehicleDto,
} from './dto/delivery-trip.dto';

const TRIP_TRANSITIONS: Record<string, string[]> = {
  DRAFT: ['MUAT', 'BATAL'],
  MUAT: ['BERANGKAT', 'BATAL'],
  BERANGKAT: ['SELESAI', 'BATAL'],
  SELESAI: [],
  BATAL: [],
};

/**
 * Fase 2 P7 — Pengiriman & armada: master kendaraan (2 unit contoh bertanda
 * provisional) + trip pengiriman (satu kendaraan, banyak Delivery Order
 * sebagai stops berurutan). Status trip DRAFT → MUAT → BERANGKAT → SELESAI.
 * Menandai stop TIBA dengan nama penerima menulis acceptance ke Delivery
 * Report DO tersebut (BAST) — tahap Order Hub pun bergerak ke DITERIMA.
 * Biaya trip (BBM/tol/lain) tercatat sebagai dasar Freight Payable.
 */
@Injectable()
export class ErpSlsDeliveryTripsService {
  constructor(private readonly prisma: PrismaService) {}

  // ── Kendaraan ───────────────────────────────────────────────────────────────
  private vehiclePresent(row: any) {
    return {
      id: row.id?.toString(),
      code: row.code,
      name: row.name,
      plateNo: row.plateNo,
      vehicleType: row.vehicleType,
      capacityKg: row.capacityKg?.toString() ?? null,
      status: row.status,
      legacyCode: row.legacyCode,
      notes: row.notes,
    };
  }

  async listVehicles() {
    const rows = await this.prisma.erpSlsVehicle.findMany({
      where: { deletedAt: null },
      orderBy: { code: 'asc' },
    });
    return { data: rows.map((r) => this.vehiclePresent(r)) };
  }

  async createVehicle(dto: CreateVehicleDto, actorId?: string) {
    const dup = await this.prisma.erpSlsVehicle.findFirst({
      where: { code: dto.code, deletedAt: null },
    });
    if (dup) throw new BadRequestException(`Kode kendaraan ${dto.code} sudah dipakai.`);
    const row = await this.prisma.erpSlsVehicle.create({
      data: {
        code: dto.code,
        name: dto.name,
        plateNo: dto.plateNo ?? null,
        vehicleType: dto.vehicleType ?? 'BOX',
        capacityKg: dto.capacityKg ? new Prisma.Decimal(dto.capacityKg) : null,
        notes: dto.notes ?? null,
        createdById: actorId ? BigInt(actorId) : null,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.vehiclePresent(row);
  }

  async updateVehicle(id: string, dto: UpdateVehicleDto, actorId?: string) {
    const row = await this.prisma.erpSlsVehicle.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Kendaraan tidak ditemukan.');
    const updated = await this.prisma.erpSlsVehicle.update({
      where: { id: row.id },
      data: {
        name: dto.name ?? row.name,
        plateNo: dto.plateNo !== undefined ? dto.plateNo : row.plateNo,
        vehicleType: dto.vehicleType ?? row.vehicleType,
        capacityKg:
          dto.capacityKg !== undefined
            ? dto.capacityKg
              ? new Prisma.Decimal(dto.capacityKg)
              : null
            : row.capacityKg,
        status: (dto.status as any) ?? row.status,
        notes: dto.notes !== undefined ? dto.notes : row.notes,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.vehiclePresent(updated);
  }

  async deleteVehicle(id: string) {
    const row = await this.prisma.erpSlsVehicle.findFirst({
      where: { id: BigInt(id), deletedAt: null },
    });
    if (!row) throw new NotFoundException('Kendaraan tidak ditemukan.');
    await this.prisma.erpSlsVehicle.update({
      where: { id: row.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }

  // ── Trip ────────────────────────────────────────────────────────────────────
  private async genDocNumber(tx: Prisma.TransactionClient): Promise<string> {
    const code = 'TRP';
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: code, deletedAt: null },
    });
    if (numbering) {
      const updated = await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: { increment: 1 } },
      });
      const seq = updated.nextNumber - 1;
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpSlsDeliveryTrip.count();
    return `TRP${String(count + 1).padStart(6, '0')}`;
  }

  private async tripOrThrow(id: bigint) {
    const trip = await this.prisma.erpSlsDeliveryTrip.findFirst({
      where: { id, deletedAt: null },
    });
    if (!trip) throw new NotFoundException('Trip tidak ditemukan.');
    return trip;
  }

  private async stopPresent(row: any) {
    const doRow = await this.prisma.erpSlsDeliveryOrder.findUnique({
      where: { id: row.deliveryOrderId },
      select: { docNumber: true, customerId: true, orderId: true },
    });
    let customerName: string | null = null;
    if (doRow?.customerId) {
      const p = await this.prisma.erpPartner.findUnique({
        where: { id: doRow.customerId },
        select: { name: true },
      });
      customerName = p?.name ?? null;
    }
    return {
      id: row.id?.toString(),
      tripId: row.tripId?.toString(),
      deliveryOrderId: row.deliveryOrderId?.toString(),
      deliveryOrderDocNumber: doRow?.docNumber ?? null,
      orderId: doRow?.orderId?.toString() ?? null,
      customerName,
      sequenceNo: row.sequenceNo,
      status: row.status,
      arrivedAt: row.arrivedAt,
      receiverName: row.receiverName,
      receiverTitle: row.receiverTitle,
      failureNote: row.failureNote,
      notes: row.notes,
    };
  }

  private async tripPresent(row: any, withStops = false) {
    const vehicle = await this.prisma.erpSlsVehicle.findUnique({
      where: { id: row.vehicleId },
      select: { code: true, name: true, plateNo: true },
    });
    const stops = await this.prisma.erpSlsDeliveryTripStop.findMany({
      where: { tripId: row.id },
      orderBy: { sequenceNo: 'asc' },
    });
    const base: any = {
      id: row.id?.toString(),
      docNumber: row.docNumber,
      branchId: row.branchId?.toString(),
      vehicleId: row.vehicleId?.toString(),
      vehicleCode: vehicle?.code ?? null,
      vehicleName: vehicle?.name ?? null,
      plateNo: vehicle?.plateNo ?? null,
      driverName: row.driverName,
      tripDate: row.tripDate ? new Date(row.tripDate).toISOString().slice(0, 10) : null,
      status: row.status,
      departedAt: row.departedAt,
      completedAt: row.completedAt,
      fuelCost: row.fuelCost?.toString(),
      tollCost: row.tollCost?.toString(),
      otherCost: row.otherCost?.toString(),
      totalCost: row.totalCost?.toString(),
      notes: row.notes,
      stopCount: stops.length,
      arrivedCount: stops.filter((s) => s.status === 'TIBA').length,
    };
    if (withStops) {
      base.stops = [] as any[];
      for (const s of stops) base.stops.push(await this.stopPresent(s));
    }
    return base;
  }

  async listTrips(query: QueryDeliveryTripsDto) {
    const page = query.page ?? 1;
    const limit = query.limit ?? 20;
    const where: Prisma.ErpSlsDeliveryTripWhereInput = { deletedAt: null };
    if (query.status) where.status = query.status as any;
    if (query.search) where.docNumber = { contains: query.search, mode: 'insensitive' };
    const [total, rows] = await this.prisma.$transaction([
      this.prisma.erpSlsDeliveryTrip.count({ where }),
      this.prisma.erpSlsDeliveryTrip.findMany({
        where,
        orderBy: { docNumber: 'desc' },
        skip: (page - 1) * limit,
        take: limit,
      }),
    ]);
    const data = [] as any[];
    for (const r of rows) data.push(await this.tripPresent(r));
    return { data, meta: { page, limit, total, totalPages: Math.max(1, Math.ceil(total / limit)) } };
  }

  async getTrip(id: string) {
    const trip = await this.tripOrThrow(BigInt(id));
    return this.tripPresent(trip, true);
  }

  async createTrip(dto: CreateDeliveryTripDto, actorId?: string) {
    const vehicle = await this.prisma.erpSlsVehicle.findFirst({
      where: { id: BigInt(dto.vehicleId), deletedAt: null },
    });
    if (!vehicle) throw new NotFoundException('Kendaraan tidak ditemukan.');
    if (vehicle.status !== 'ACTIVE') {
      throw new BadRequestException(`Kendaraan ${vehicle.code} berstatus ${vehicle.status}.`);
    }
    let branchId: bigint;
    if (dto.branchId) branchId = BigInt(dto.branchId);
    else {
      const branch = await this.prisma.erpBranch.findFirst({
        where: { deletedAt: null },
        orderBy: { id: 'asc' },
        select: { id: true },
      });
      if (!branch) throw new BadRequestException('Branch tidak ditemukan.');
      branchId = branch.id;
    }
    const row = await this.prisma.$transaction(async (tx) => {
      const docNumber = await this.genDocNumber(tx);
      return tx.erpSlsDeliveryTrip.create({
        data: {
          docNumber,
          branchId,
          vehicleId: vehicle.id,
          driverName: dto.driverName ?? null,
          tripDate: new Date(dto.tripDate),
          notes: dto.notes ?? null,
          createdById: actorId ? BigInt(actorId) : null,
          updatedById: actorId ? BigInt(actorId) : null,
        },
      });
    });
    return this.tripPresent(row, true);
  }

  async updateTrip(id: string, dto: UpdateDeliveryTripDto, actorId?: string) {
    const trip = await this.tripOrThrow(BigInt(id));
    if (trip.status !== 'DRAFT' && trip.status !== 'MUAT') {
      throw new BadRequestException(`Trip berstatus ${trip.status} tidak dapat diubah.`);
    }
    const updated = await this.prisma.erpSlsDeliveryTrip.update({
      where: { id: trip.id },
      data: {
        vehicleId: dto.vehicleId ? BigInt(dto.vehicleId) : trip.vehicleId,
        tripDate: dto.tripDate ? new Date(dto.tripDate) : trip.tripDate,
        driverName: dto.driverName !== undefined ? dto.driverName : trip.driverName,
        notes: dto.notes !== undefined ? dto.notes : trip.notes,
        updatedById: actorId ? BigInt(actorId) : null,
      },
    });
    return this.tripPresent(updated, true);
  }

  async setCosts(id: string, dto: SetTripCostsDto) {
    const trip = await this.tripOrThrow(BigInt(id));
    if (trip.status === 'BATAL') throw new BadRequestException('Trip BATAL tidak dapat diubah biayanya.');
    const fuel = dto.fuelCost !== undefined ? new Prisma.Decimal(dto.fuelCost) : trip.fuelCost;
    const toll = dto.tollCost !== undefined ? new Prisma.Decimal(dto.tollCost) : trip.tollCost;
    const other = dto.otherCost !== undefined ? new Prisma.Decimal(dto.otherCost) : trip.otherCost;
    const updated = await this.prisma.erpSlsDeliveryTrip.update({
      where: { id: trip.id },
      data: { fuelCost: fuel, tollCost: toll, otherCost: other, totalCost: fuel.add(toll).add(other) },
    });
    return this.tripPresent(updated, true);
  }

  async setTripStatus(id: string, status: string) {
    const trip = await this.tripOrThrow(BigInt(id));
    const allowed = TRIP_TRANSITIONS[trip.status] ?? [];
    if (!allowed.includes(status)) {
      throw new BadRequestException(
        `Transisi trip ${trip.status} → ${status} tidak valid (pilihan: ${allowed.join(', ') || 'tidak ada'}).`,
      );
    }
    if (status === 'BERANGKAT') {
      const count = await this.prisma.erpSlsDeliveryTripStop.count({ where: { tripId: trip.id } });
      if (!count) throw new BadRequestException('Trip belum punya stop — tambahkan Delivery Order dulu.');
    }
    if (status === 'SELESAI') {
      const pending = await this.prisma.erpSlsDeliveryTripStop.count({
        where: { tripId: trip.id, status: 'MENUNGGU' },
      });
      if (pending) {
        throw new BadRequestException(`Masih ada ${pending} stop MENUNGGU — tandai TIBA/GAGAL dulu.`);
      }
    }
    const now = new Date();
    const updated = await this.prisma.erpSlsDeliveryTrip.update({
      where: { id: trip.id },
      data: {
        status: status as any,
        departedAt: status === 'BERANGKAT' ? (trip.departedAt ?? now) : trip.departedAt,
        completedAt: status === 'SELESAI' ? now : trip.completedAt,
      },
    });
    return this.tripPresent(updated, true);
  }

  async deleteTrip(id: string) {
    const trip = await this.tripOrThrow(BigInt(id));
    if (trip.status !== 'DRAFT') {
      throw new BadRequestException('Hanya trip DRAFT yang dapat dihapus.');
    }
    await this.prisma.erpSlsDeliveryTrip.update({
      where: { id: trip.id },
      data: { deletedAt: new Date() },
    });
    return { success: true };
  }

  // ── Stops ───────────────────────────────────────────────────────────────────
  async addStop(tripId: string, deliveryOrderId: string) {
    const trip = await this.tripOrThrow(BigInt(tripId));
    if (trip.status !== 'DRAFT' && trip.status !== 'MUAT') {
      throw new BadRequestException(`Stop hanya dapat ditambah saat trip DRAFT/MUAT (kini ${trip.status}).`);
    }
    const doRow = await this.prisma.erpSlsDeliveryOrder.findFirst({
      where: { id: BigInt(deliveryOrderId), deletedAt: null },
      select: { id: true, docNumber: true },
    });
    if (!doRow) throw new NotFoundException('Delivery Order tidak ditemukan.');
    const clash = await this.prisma.erpSlsDeliveryTripStop.findFirst({
      where: {
        deliveryOrderId: doRow.id,
        trip: { status: { in: ['DRAFT', 'MUAT', 'BERANGKAT'] as any }, deletedAt: null },
      },
      select: { id: true, tripId: true },
    });
    if (clash) {
      throw new BadRequestException(`DO ${doRow.docNumber} sudah masuk trip aktif lain.`);
    }
    const last = await this.prisma.erpSlsDeliveryTripStop.findFirst({
      where: { tripId: trip.id },
      orderBy: { sequenceNo: 'desc' },
      select: { sequenceNo: true },
    });
    const row = await this.prisma.erpSlsDeliveryTripStop.create({
      data: {
        tripId: trip.id,
        deliveryOrderId: doRow.id,
        sequenceNo: (last?.sequenceNo ?? 0) + 1,
      },
    });
    return this.stopPresent(row);
  }

  async removeStop(tripId: string, stopId: string) {
    const trip = await this.tripOrThrow(BigInt(tripId));
    if (trip.status !== 'DRAFT' && trip.status !== 'MUAT') {
      throw new BadRequestException('Stop hanya dapat dihapus saat trip DRAFT/MUAT.');
    }
    const stop = await this.prisma.erpSlsDeliveryTripStop.findFirst({
      where: { id: BigInt(stopId), tripId: trip.id },
    });
    if (!stop) throw new NotFoundException('Stop tidak ditemukan.');
    await this.prisma.erpSlsDeliveryTripStop.delete({ where: { id: stop.id } });
    return { success: true };
  }

  private async stopOrThrow(tripId: bigint, stopId: bigint) {
    const stop = await this.prisma.erpSlsDeliveryTripStop.findFirst({
      where: { id: stopId, tripId },
    });
    if (!stop) throw new NotFoundException('Stop tidak ditemukan.');
    return stop;
  }

  async arriveStop(tripId: string, stopId: string, dto: ArriveTripStopDto) {
    const trip = await this.tripOrThrow(BigInt(tripId));
    if (trip.status !== 'BERANGKAT') {
      throw new BadRequestException('Stop hanya dapat ditandai TIBA saat trip BERANGKAT.');
    }
    const stop = await this.stopOrThrow(trip.id, BigInt(stopId));
    if (stop.status !== 'MENUNGGU') {
      throw new BadRequestException(`Stop sudah berstatus ${stop.status}.`);
    }
    const now = new Date();
    const updated = await this.prisma.erpSlsDeliveryTripStop.update({
      where: { id: stop.id },
      data: {
        status: 'TIBA',
        arrivedAt: now,
        receiverName: dto.receiverName,
        receiverTitle: dto.receiverTitle ?? null,
      },
    });
    // Umpan BAST: acceptance pada Delivery Report DO ini → hub DITERIMA.
    const bast = await this.prisma.erpSlsDeliveryReport.updateMany({
      where: { deliveryOrderId: stop.deliveryOrderId, deletedAt: null, acceptedAt: null },
      data: {
        acceptedAt: now,
        acceptedByName: dto.receiverName,
        acceptedByTitle: dto.receiverTitle ?? null,
      },
    });
    const presented = await this.stopPresent(updated);
    return { ...presented, bastUpdated: bast.count };
  }

  async failStop(tripId: string, stopId: string, dto: FailTripStopDto) {
    const trip = await this.tripOrThrow(BigInt(tripId));
    if (trip.status !== 'BERANGKAT') {
      throw new BadRequestException('Stop hanya dapat ditandai GAGAL saat trip BERANGKAT.');
    }
    const stop = await this.stopOrThrow(trip.id, BigInt(stopId));
    if (stop.status !== 'MENUNGGU') {
      throw new BadRequestException(`Stop sudah berstatus ${stop.status}.`);
    }
    const updated = await this.prisma.erpSlsDeliveryTripStop.update({
      where: { id: stop.id },
      data: { status: 'GAGAL', failureNote: dto.failureNote },
    });
    return this.stopPresent(updated);
  }
}
