// Header SearchSelect loaders for the inventory stock-adjustment form.
// Warehouse + Branch reuse the Item form loaders. No currency on inventory
// adjustments; the adjustment header only needs branch + warehouse pickers.

export {
  loadWarehouseOptions,
  loadBranchOptions,
} from '@/components/pages/items-form-lookups';
