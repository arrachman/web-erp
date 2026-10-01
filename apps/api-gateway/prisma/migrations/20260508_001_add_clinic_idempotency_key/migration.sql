-- CreateTable
CREATE TABLE "clinic_idempotency_key" (
    "key" TEXT NOT NULL,
    "response" JSONB NOT NULL,
    "status_code" INTEGER NOT NULL,
    "actor_id" INTEGER,
    "created_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "clinic_idempotency_key_pkey" PRIMARY KEY ("key")
);

-- CreateIndex
CREATE INDEX "clinic_idempotency_key_created_at_idx" ON "clinic_idempotency_key"("created_at");
