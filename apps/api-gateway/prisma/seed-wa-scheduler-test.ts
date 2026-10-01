/**
 * Seed 3 test booking untuk uji WA Scheduler dari UI admin/notif-wa.
 *
 * Jalankan:
 *   cd apps/api-gateway
 *   npx ts-node prisma/seed-wa-scheduler-test.ts [nomor_wa]
 *
 * Contoh:
 *   npx ts-node prisma/seed-wa-scheduler-test.ts +6281234567890
 *   npx ts-node prisma/seed-wa-scheduler-test.ts  ← pakai nomor default
 *
 * Membuat / upsert:
 *   - 1 test client (phoneWa = nomor di atas)
 *   - 3 booking test:
 *       [H-1]         besok 09:00 WIB, status confirmed
 *       [M-30]        sekarang +30 menit, status confirmed
 *       [FEEDBACK H+1] kemarin 14:00 WIB, status completed
 *
 * Setelah script jalan, buka UI → Test Scheduler WA:
 *   - Pilih tipe, kosongkan "Nomor WA Test" (booking sudah ada), klik Jalankan
 *   - Atau pilih tipe + isi nomor WA → buat booking baru sekaligus kirim
 */

import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const TZ = 'Asia/Jakarta';
const TEST_CLIENT_NAME = 'Test WA Scheduler';

function localDateAtMidnight(dateStr: string, tz: string): Date {
  const [y, m, d] = dateStr.split('-').map(Number);
  const asLocal = new Date(
    new Intl.DateTimeFormat('en-CA', {
      timeZone: tz,
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    })
      .format(new Date())
      .replace(/\//g, '-'),
  );
  // Simple: build ISO string at midnight and convert via offset
  const naive = new Date(`${dateStr}T00:00:00`);
  // Get the offset at that date in target TZ
  const tzDate = new Date(naive.toLocaleString('en-US', { timeZone: tz }));
  const offset = naive.getTime() - tzDate.getTime();
  return new Date(naive.getTime() + offset);
}

function getTomorrowMidnight(): Date {
  const now = new Date();
  const todayStr = now.toLocaleDateString('en-CA', { timeZone: TZ }); // YYYY-MM-DD
  const [y, m, d] = todayStr.split('-').map(Number);
  const todayLocal = localDateAtMidnight(todayStr, TZ);
  return new Date(todayLocal.getTime() + 86400000);
}

function getYesterdayMidnight(): Date {
  const now = new Date();
  const todayStr = now.toLocaleDateString('en-CA', { timeZone: TZ });
  const todayLocal = localDateAtMidnight(todayStr, TZ);
  return new Date(todayLocal.getTime() - 86400000);
}

async function main() {
  const rawPhone = process.argv[2] ?? '+628000000000';
  // Normalize: strip non-digit, prepend +62 if starts with 0
  let phone = rawPhone.replace(/\s/g, '');
  if (phone.startsWith('08')) phone = '+62' + phone.slice(1);
  if (!phone.startsWith('+')) phone = '+62' + phone;

  console.log(`\n📱 Test phone: ${phone}`);

  // Upsert test client
  let client = await prisma.clinicClient.findFirst({
    where: { phoneWa: phone, deletedAt: null },
  });
  if (!client) {
    client = await prisma.clinicClient.create({
      data: {
        name: TEST_CLIENT_NAME,
        gender: 'L',
        phoneWa: phone,
        waOptedOut: false,
        isActive: true,
      },
    });
    console.log(`✅ Client dibuat: ${client.name} (id=${client.id})`);
  } else {
    console.log(`♻️  Client sudah ada: ${client.name} (id=${client.id})`);
  }

  // Find prerequisites
  const psikolog = await prisma.clinicPsikologProfile.findFirst({
    where: { isActive: true },
    select: { userId: true },
  });
  if (!psikolog) throw new Error('❌ Tidak ada psikolog aktif');

  const service = await prisma.clinicService.findFirst({
    where: { isActive: true, deletedAt: null },
    select: { id: true, durationMinutes: true },
  });
  if (!service) throw new Error('❌ Tidak ada layanan aktif');

  const room = await prisma.clinicRoom.findFirst({
    where: { isActive: true, deletedAt: null },
    select: { id: true },
  });
  if (!room) throw new Error('❌ Tidak ada ruangan aktif');

  const durMs = service.durationMinutes * 60000;
  const now = new Date();

  // ──────────────────────────────────────────────────────
  // 1. H-1: besok jam 09:00 WIB, status confirmed
  // ──────────────────────────────────────────────────────
  const tomorrowMid = getTomorrowMidnight();
  const h1Start = new Date(tomorrowMid.getTime() + 9 * 3600000); // 09:00 WIB
  const h1End = new Date(h1Start.getTime() + durMs);

  const bookingH1 = await prisma.clinicBooking.create({
    data: {
      clientId: client.id,
      serviceId: service.id,
      psikologUserId: psikolog.userId,
      roomId: room.id,
      scheduledStart: h1Start,
      scheduledEnd: h1End,
      status: 'confirmed',
      confirmedAt: now,
      bufferOverride: true,
      notes: '[TEST H-1] Dibuat oleh seed-wa-scheduler-test',
    },
  });
  console.log(`\n📅 [H-1]         Booking #${bookingH1.id}`);
  console.log(`   scheduledStart: ${h1Start.toLocaleString('id-ID', { timeZone: TZ })} WIB`);
  console.log(`   status: confirmed`);

  // ──────────────────────────────────────────────────────
  // 2. M-30: sekarang + 30 menit, status confirmed
  // ──────────────────────────────────────────────────────
  const m30Start = new Date(now.getTime() + 30 * 60000);
  const m30End = new Date(m30Start.getTime() + durMs);

  const bookingM30 = await prisma.clinicBooking.create({
    data: {
      clientId: client.id,
      serviceId: service.id,
      psikologUserId: psikolog.userId,
      roomId: room.id,
      scheduledStart: m30Start,
      scheduledEnd: m30End,
      status: 'confirmed',
      confirmedAt: now,
      bufferOverride: true,
      notes: '[TEST M-30] Dibuat oleh seed-wa-scheduler-test',
    },
  });
  console.log(`\n⏱️  [M-30]        Booking #${bookingM30.id}`);
  console.log(`   scheduledStart: ${m30Start.toLocaleString('id-ID', { timeZone: TZ })} WIB`);
  console.log(`   status: confirmed`);
  console.log(`   ⚠️  SEGERA test M-30 dalam 5 menit atau booking tidak masuk window`);

  // ──────────────────────────────────────────────────────
  // 3. Feedback H+1: kemarin 14:00 WIB, status completed
  // ──────────────────────────────────────────────────────
  const yesterdayMid = getYesterdayMidnight();
  const fbStart = new Date(yesterdayMid.getTime() + 14 * 3600000); // 14:00 WIB kemarin
  const fbEnd = new Date(fbStart.getTime() + durMs);
  const fbCompleted = new Date(fbEnd.getTime() + 5 * 60000); // ~5 menit setelah selesai

  const bookingFb = await prisma.clinicBooking.create({
    data: {
      clientId: client.id,
      serviceId: service.id,
      psikologUserId: psikolog.userId,
      roomId: room.id,
      scheduledStart: fbStart,
      scheduledEnd: fbEnd,
      status: 'completed',
      confirmedAt: fbStart,
      completedAt: fbCompleted,
      bufferOverride: true,
      notes: '[TEST FEEDBACK-H1] Dibuat oleh seed-wa-scheduler-test',
    },
  });
  console.log(`\n💬 [FEEDBACK H+1] Booking #${bookingFb.id}`);
  console.log(`   scheduledStart: ${fbStart.toLocaleString('id-ID', { timeZone: TZ })} WIB`);
  console.log(`   completedAt:    ${fbCompleted.toLocaleString('id-ID', { timeZone: TZ })} WIB`);
  console.log(`   status: completed`);

  console.log(`
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ 3 booking test berhasil dibuat!

Langkah selanjutnya di UI /admin/notif-wa → Test Scheduler WA:

  1. [H-1]          → pilih "Pengingat H-1"         → Jalankan (booking #${bookingH1.id})
  2. [M-30]         → pilih "Pengingat 30 Menit"     → SEGERA Jalankan (booking #${bookingM30.id})
  3. [Feedback H+1] → pilih "Form Feedback H+1"      → Jalankan (booking #${bookingFb.id})

  Tips: Centang "Dry run" dulu untuk memverifikasi kandidat terdeteksi,
        lalu uncheck dan Jalankan untuk kirim WA asli.
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(() => prisma.$disconnect());
