// ERP Item resource API — barrel re-exporting the public surface.
// Pure restructure: import path `@/lib/api/items` is unchanged for consumers.

// Types
export type {
  ErpItemType,
  ErpCostingMethod,
  ItemPriceTier,
  ItemDistributorRow,
  ItemWarehouseStockRow,
  ItemOthersData,
  ItemCustomData,
  ItemMetadata,
  ErpItem,
} from './items/types';

export type { CreateItemPayload, UpdateItemPayload } from './items/payloads';

export type { ItemMediaKind, ItemMedia } from './items/media';
export type { ItemAttachment } from './items/attachments';
export type { ItemPurchaseAutoFill } from './items/purchase-auto-fill';

// Functions — CRUD
export {
  listItems,
  createItem,
  updateItem,
  deleteItem,
  bulkUpdateItemStatus,
  bulkDeleteItems,
} from './items/crud';

// Functions — Media
export {
  listItemMedia,
  uploadItemMedia,
  setPrimaryItemMedia,
  deleteItemMedia,
  itemMediaFileUrl,
} from './items/media';

// Functions — Attachments
export {
  listItemAttachments,
  uploadItemAttachment,
  updateItemAttachmentNote,
  deleteItemAttachment,
  itemAttachmentFileUrl,
} from './items/attachments';

// Functions — Purchase auto-fill
export { getItemForPurchaseAutoFill } from './items/purchase-auto-fill';