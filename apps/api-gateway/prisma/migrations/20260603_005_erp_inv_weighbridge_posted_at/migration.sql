-- ErpInvWeighbridgeTicket.postedAt was added to the Prisma schema during the RW
-- build but never migrated, so inv_weighbridge_tickets lacks the column and every
-- Prisma read (list + reports) fails with P2022. Add it (idempotent).
ALTER TABLE "inv_weighbridge_tickets" ADD COLUMN IF NOT EXISTS "posted_at" TIMESTAMPTZ(6);
