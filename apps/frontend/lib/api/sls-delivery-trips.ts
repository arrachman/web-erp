// Fase 2 P7 — Pengiriman & Armada API (kendaraan + trip + stops).
// Endpoints: /sls/vehicles, /sls/delivery-trips

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

export const VEHICLE_STATUSES = ['ACTIVE', 'MAINTENANCE', 'INACTIVE'] as const;
export type VehicleStatus = (typeof VEHICLE_STATUSES)[number];
export type TripStatus = 'DRAFT' | 'MUAT' | 'BERANGKAT' | 'SELESAI' | 'BATAL';
export type TripStopStatus = 'MENUNGGU' | 'TIBA' | 'GAGAL';

export interface SlsVehicle {
  id: string;
  code: string;
  name: string;
  plateNo?: string | null;
  vehicleType: string;
  capacityKg?: string | null;
  status: VehicleStatus;
  legacyCode?: string | null;
}

export interface TripStop {
  id: string;
  tripId: string;
  deliveryOrderId: string;
  deliveryOrderDocNumber?: string | null;
  orderId?: string | null;
  customerName?: string | null;
  sequenceNo: number;
  status: TripStopStatus;
  arrivedAt?: string | null;
  receiverName?: string | null;
  receiverTitle?: string | null;
  failureNote?: string | null;
  bastUpdated?: number;
}

export interface DeliveryTrip {
  id: string;
  docNumber: string;
  vehicleId: string;
  vehicleCode?: string | null;
  vehicleName?: string | null;
  plateNo?: string | null;
  driverName?: string | null;
  tripDate: string;
  status: TripStatus;
  departedAt?: string | null;
  completedAt?: string | null;
  fuelCost: string;
  tollCost: string;
  otherCost: string;
  totalCost: string;
  stopCount: number;
  arrivedCount: number;
  stops?: TripStop[];
}

export async function listVehicles(): Promise<SlsVehicle[]> {
  const res = await apiGet<ApiResponse<SlsVehicle[]>>(`/sls/vehicles`);
  return (res as any)?.data ?? [];
}

export async function createVehicle(payload: {
  code: string;
  name: string;
  plateNo?: string;
  vehicleType?: string;
  capacityKg?: string;
}): Promise<SlsVehicle> {
  const res = await apiPost<ApiResponse<SlsVehicle>>(`/sls/vehicles`, payload);
  return (res as any).data ?? res;
}

export async function updateVehicle(
  id: string,
  payload: Partial<{ status: VehicleStatus; name: string }>,
): Promise<SlsVehicle> {
  const res = await apiPatch<ApiResponse<SlsVehicle>>(`/sls/vehicles/${id}`, payload);
  return (res as any).data ?? res;
}

export async function deleteVehicle(id: string): Promise<void> {
  await apiDelete(`/sls/vehicles/${id}`);
}

export async function listTrips(): Promise<DeliveryTrip[]> {
  const res = await apiGet<PaginatedResponse<DeliveryTrip>>(`/sls/delivery-trips?limit=100`);
  return (res as any)?.data ?? [];
}

export async function getTrip(id: string): Promise<DeliveryTrip> {
  const res = await apiGet<ApiResponse<DeliveryTrip>>(`/sls/delivery-trips/${id}`);
  return (res as any).data ?? res;
}

export async function createTrip(payload: {
  tripDate: string;
  vehicleId: string;
  driverName?: string;
}): Promise<DeliveryTrip> {
  const res = await apiPost<ApiResponse<DeliveryTrip>>(`/sls/delivery-trips`, payload);
  return (res as any).data ?? res;
}

export async function setTripStatus(id: string, status: TripStatus): Promise<DeliveryTrip> {
  const res = await apiPost<ApiResponse<DeliveryTrip>>(`/sls/delivery-trips/${id}/status`, { status });
  return (res as any).data ?? res;
}

export async function setTripCosts(
  id: string,
  payload: { fuelCost?: string; tollCost?: string; otherCost?: string },
): Promise<DeliveryTrip> {
  const res = await apiPost<ApiResponse<DeliveryTrip>>(`/sls/delivery-trips/${id}/costs`, payload);
  return (res as any).data ?? res;
}

export async function addTripStop(tripId: string, deliveryOrderId: string): Promise<TripStop> {
  const res = await apiPost<ApiResponse<TripStop>>(`/sls/delivery-trips/${tripId}/stops`, { deliveryOrderId });
  return (res as any).data ?? res;
}

export async function removeTripStop(tripId: string, stopId: string): Promise<void> {
  await apiDelete(`/sls/delivery-trips/${tripId}/stops/${stopId}`);
}

export async function arriveTripStop(
  tripId: string,
  stopId: string,
  payload: { receiverName: string; receiverTitle?: string },
): Promise<TripStop> {
  const res = await apiPost<ApiResponse<TripStop>>(
    `/sls/delivery-trips/${tripId}/stops/${stopId}/arrive`,
    payload,
  );
  return (res as any).data ?? res;
}

export async function failTripStop(
  tripId: string,
  stopId: string,
  failureNote: string,
): Promise<TripStop> {
  const res = await apiPost<ApiResponse<TripStop>>(
    `/sls/delivery-trips/${tripId}/stops/${stopId}/fail`,
    { failureNote },
  );
  return (res as any).data ?? res;
}
