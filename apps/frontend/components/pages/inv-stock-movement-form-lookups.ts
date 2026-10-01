// Header SearchSelect loaders for the inventory stock-movement form.
// Warehouse (source/transit/destination) + Branch + Location reuse the Item
// form loaders. Requested-partner = partners with type kind SUPPLIER (the unit/vendor
// a Material Request is addressed to). No currency on inventory movements.

export {
  loadWarehouseOptions,
  loadBranchOptions,
  loadLocationOptions,
  loadSupplierOptions as loadRequestedPartnerOptions,
} from '@/components/pages/items-form-lookups';
