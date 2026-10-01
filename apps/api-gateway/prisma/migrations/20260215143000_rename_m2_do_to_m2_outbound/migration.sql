ALTER TABLE "m2_do" RENAME TO "m2_outbound";
ALTER TABLE "m2_do_detail" RENAME TO "m2_outbound_detail";

ALTER TABLE "m2_outbound" RENAME CONSTRAINT "m2_do_pkey" TO "m2_outbound_pkey";
ALTER TABLE "m2_outbound" RENAME CONSTRAINT "m2_do_customer_id_fkey" TO "m2_outbound_customer_id_fkey";
ALTER TABLE "m2_outbound" RENAME CONSTRAINT "m2_do_destination_city_id_fkey" TO "m2_outbound_destination_city_id_fkey";

ALTER TABLE "m2_outbound_detail" RENAME CONSTRAINT "m2_do_detail_pkey" TO "m2_outbound_detail_pkey";
ALTER TABLE "m2_outbound_detail" RENAME CONSTRAINT "m2_do_detail_do_id_fkey" TO "m2_outbound_detail_do_id_fkey";
ALTER TABLE "m2_outbound_detail" RENAME CONSTRAINT "m2_do_detail_item_id_fkey" TO "m2_outbound_detail_item_id_fkey";

ALTER INDEX "m2_do_uuid_key" RENAME TO "m2_outbound_uuid_key";
ALTER INDEX "m2_do_report_no_key" RENAME TO "m2_outbound_report_no_key";
ALTER INDEX "m2_do_customer_id_idx" RENAME TO "m2_outbound_customer_id_idx";
ALTER INDEX "m2_do_shipping_date_idx" RENAME TO "m2_outbound_shipping_date_idx";
ALTER INDEX "m2_do_do_received_date_idx" RENAME TO "m2_outbound_do_received_date_idx";
ALTER INDEX "m2_do_detail_uuid_key" RENAME TO "m2_outbound_detail_uuid_key";
ALTER INDEX "m2_do_detail_do_id_idx" RENAME TO "m2_outbound_detail_do_id_idx";
ALTER INDEX "m2_do_detail_item_id_idx" RENAME TO "m2_outbound_detail_item_id_idx";
ALTER INDEX "m2_do_detail_do_id_line_no_key" RENAME TO "m2_outbound_detail_do_id_line_no_key";
