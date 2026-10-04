// Daftar Kebutuhan (keranjang) portal — disimpan lokal per perangkat.
import type { CatalogItem } from './api';

export interface CartLine {
  itemId: string;
  name: string;
  code: string;
  unit: string | null;
  salePrice: string;
  qty: number;
}

const CART_KEY = 'portal_cart';
const REF_KEY = 'portal_cart_ref';

export function loadCart(): CartLine[] {
  try {
    return JSON.parse(window.localStorage.getItem(CART_KEY) ?? '[]');
  } catch {
    return [];
  }
}

export function saveCart(lines: CartLine[]) {
  window.localStorage.setItem(CART_KEY, JSON.stringify(lines));
  window.dispatchEvent(new CustomEvent('cart-changed'));
}

export function addToCart(item: CatalogItem, qty = 1) {
  const lines = loadCart();
  const existing = lines.find((l) => l.itemId === item.id);
  if (existing) existing.qty += qty;
  else
    lines.push({
      itemId: item.id,
      name: item.name,
      code: item.code,
      unit: item.unit,
      // Harga yang berlaku untuk sekolah akun (kontrak W7 bila ada).
      salePrice: item.price ?? item.salePrice,
      qty,
    });
  if (!window.localStorage.getItem(REF_KEY)) {
    window.localStorage.setItem(REF_KEY, crypto.randomUUID());
  }
  saveCart(lines);
}

export function cartRef(): string {
  let ref = window.localStorage.getItem(REF_KEY);
  if (!ref) {
    ref = crypto.randomUUID();
    window.localStorage.setItem(REF_KEY, ref);
  }
  return ref;
}

export function clearCart() {
  window.localStorage.removeItem(CART_KEY);
  window.localStorage.removeItem(REF_KEY);
  window.dispatchEvent(new CustomEvent('cart-changed'));
}

export function cartCount(): number {
  return loadCart().reduce((s, l) => s + l.qty, 0);
}
