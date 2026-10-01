-- HPP Terakhir: net landed cost from the most recent posted purchase (Goods Receipt).
-- Distinct from average_cost (HPP Rata-rata, moving average) and standard_cost
-- (legacy manual "HPP Update", now removed from the UI). Additive, defaults 0.

ALTER TABLE "md_items" ADD COLUMN "last_hpp" DECIMAL(19,4) NOT NULL DEFAULT 0;
