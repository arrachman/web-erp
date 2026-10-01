// Header SearchSelect loaders for the inventory opening-stock form.
// Branch + Location + Warehouse + Currency reuse the Item form loaders (same
// master list APIs). Opening stock carries a currency + exchange rate, unlike
// stock movements.

export {
  loadBranchOptions,
  loadLocationOptions,
  loadWarehouseOptions,
  loadCurrencyOptions,
} from '@/components/pages/items-form-lookups';
