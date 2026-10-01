# api-gateway — Agent Guide

Backend API Gateway **khusus Senti ERP** (web-erp) — dipindah dari
`sentient-factory/apps/api-gateway`, dibersihkan dari modul MDP/clinic/HR.

## Stack
- **NestJS 10** (Express platform), TypeScript strict.
- **Prisma 5** (PostgreSQL).
- Auth: `@nestjs/jwt` + Passport.
- Swagger: `@nestjs/swagger` (auto-generated).
- Mail: `nodemailer`.

## Port
3103 (lihat `config/ports.json` di root). Jangan hardcode — baca dari env `API_GATEWAY_PORT`.

## Perintah
```bash
npm run dev               # nest start --watch
npm run build && npm start
npm run lint && npm run typecheck && npm test
npm run db:migrate        # prisma migrate dev
npm run db:generate       # regen Prisma client (WAJIB setelah edit schema.prisma)
npm run db:seed
npm run db:studio
```

## Konvensi
- Struktur **module-based** NestJS: `src/<feature>/{controller,service,dto,module}.ts`.
- DTO pakai `class-validator` + `class-transformer`. Validasi global aktif.
- Setiap endpoint **versi** di path: `/api/v1/...`.
- Error: lempar `HttpException` turunan; jangan return `{error}` manual.
- Logger: gunakan `@sentient-factory/logger` (package), bukan `console.log`.

## Database (Prisma)
- Schema: **multi-file** di `prisma/schema/` (fitur `prismaSchemaFolder`, preview di Prisma 5.22) — **SSOT**. Dipecah per-domain: `platform`/`wms` + `erp-*` (core/md/fin/sls/pur/inv/mfg/fa/pos/pln) + `mdp-mes` + `enums` + `datasource`. Tambah/ubah model di file domain yang sesuai (`@@map` prefix menentukan domain), lalu `npm run db:generate`. Prisma auto-detect folder; jangan buat `prisma/schema.prisma` lagi. Detail & alasan: `prisma/SCHEMA-SPLIT-PLAN.md`.
- Migrasi dev: `npm run db:migrate -- --name <slug>`. **Jangan** edit migrasi yang sudah dipush.
- Seed dev: `prisma/seed.ts`. Idempoten.
- Backfill scripts (`backfill-*.ts`) hanya jalan manual; jangan masukkan ke startup.

## Hal yang sering bikin masalah
- Lupa `db:generate` setelah ubah `schema.prisma` → TS error di service.
- Push migration tanpa nama → `prisma migrate dev` interaktif, gagal di CI.
- Folder `dist/`, `dist_root_*`, `node_modules.root_owned_backup_*` — **artefak**, jangan dimodif/commit.
- `temp/` dan `sql-templates/` — scratchpad; jangan import dari `src/`.

## Testing
- Jest (`*.spec.ts`). Pakai `Test.createTestingModule` dari `@nestjs/testing`.
- Integrasi DB → pakai schema test terpisah, bukan mock Prisma client.

## Jangan disentuh tanpa diminta
- `prisma/migrations/` (yang sudah ada).
- File backup `*_backup_*`, `*_locked_*`.
- `reset-db.ts` — destruktif.

## Worktree Policy (VPS-wide)

- **Do not use Git worktrees on this VPS.** Work directly in the active workspace/checkout.
- Do not create, enter, recommend, or require a worktree for any task, including background jobs.
- Use the current branch, or create a normal Git branch in the same checkout when isolation is needed.
