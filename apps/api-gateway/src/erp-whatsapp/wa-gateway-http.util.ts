/** Client HTTP ke wa-gateway self-hosted (API kompatibel Fonnte). */

export type GatewayResponse = {
  ok: boolean;
  status: number;
  json: Record<string, unknown>;
};

/** POST form-urlencoded ke gateway; ok = HTTP ok && json.status !== false. */
export async function gatewayPost(
  apiUrl: string,
  path: string,
  authToken: string,
  body: Record<string, string>,
): Promise<GatewayResponse> {
  const form = new URLSearchParams();
  for (const [k, v] of Object.entries(body)) form.set(k, v);
  const res = await fetch(`${apiUrl}${path}`, {
    method: 'POST',
    headers: {
      Authorization: authToken,
      'Content-Type': 'application/x-www-form-urlencoded',
    },
    body: form.toString(),
  });
  let json: Record<string, unknown> = {};
  try {
    json = (await res.json()) as Record<string, unknown>;
  } catch {
    // abaikan parse error — json tetap kosong
  }
  return { ok: res.ok && json.status !== false, status: res.status, json };
}

/** Ambil array `data` device dari payload gateway (toleran shape tak terduga). */
export function extractDeviceList(json: unknown): Array<Record<string, unknown>> {
  return Array.isArray((json as { data?: unknown }).data)
    ? ((json as { data: Array<Record<string, unknown>> }).data as Array<Record<string, unknown>>)
    : [];
}

export type WaDeviceView = {
  name?: string;
  device?: string;
  status?: string;
  token?: string;
  autoread?: string;
  isActive: boolean;
};

/** Normalisasi satu raw device gateway ke view + tandai yang aktif. */
export function mapGatewayDevice(
  d: Record<string, unknown>,
  activeToken: string | null,
): WaDeviceView {
  const token = typeof d.token === 'string' ? d.token : undefined;
  return {
    name: typeof d.name === 'string' ? d.name : undefined,
    device: typeof d.device === 'string' ? d.device : undefined,
    status: typeof d.status === 'string' ? d.status : undefined,
    token,
    autoread: typeof d.autoread === 'string' ? d.autoread : undefined,
    isActive: !!activeToken && token === activeToken,
  };
}
