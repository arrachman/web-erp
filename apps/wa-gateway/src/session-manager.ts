import fs from 'node:fs';
import path from 'node:path';
import {
  makeWASocket,
  useMultiFileAuthState,
  Browsers,
  type WASocket,
  type ConnectionState,
} from '@whiskeysockets/baileys';
import QRCode from 'qrcode';
import { config } from './config';
import { logger, baileysLogger } from './logger';
import { registry } from './registry';
import { emitWebhook } from './webhook';
import { getWaVersion } from './wa-version';
import { isFatalDisconnect, reconnectDelayMs } from './reconnect-policy';
import { sendTextMessage, sendDocumentMessage } from './session-send';
import { startWatchdog } from './session-watchdog';
import {
  type Session,
  type SessionHealth,
  PRESENCE_REASSERT_MS,
  CONNECT_TIMEOUT_MS,
  QR_FIRST_MAX_AGE_MS,
  QR_ROTATED_MAX_AGE_MS,
  QR_WAIT_TIMEOUT_MS,
  SOCKET_ALIVE_MS,
} from './session-types';

const delay = (ms: number) => new Promise((r) => setTimeout(r, ms));

/**
 * Kelola lifecycle koneksi Baileys per device token.
 *
 * Konfigurasi anti-suppress notifikasi (inti masalah Fonnte):
 *  - `markOnlineOnConnect: false` → companion device tetap "offline" → HP tetap
 *    jadi target push notification (banner/suara/getar normal).
 *  - tidak pernah panggil readMessages / sendReceipt('read') → setara autoread off.
 *  - tidak subscribe presence.
 */
class SessionManager {
  private sessions = new Map<string, Session>();

  private getOrInit(token: string): Session {
    let s = this.sessions.get(token);
    if (!s) {
      s = {
        token,
        status: 'disconnect',
        starting: false,
        manualLogout: false,
        reconnectAttempts: 0,
        lastProgressAt: Date.now(),
      };
      this.sessions.set(token, s);
    }
    return s;
  }

  /** Tandai ada kemajuan state — menahan watchdog menganggap sesi macet. */
  private markProgress(s: Session): void {
    s.lastProgressAt = Date.now();
  }

  private sessionDir(token: string): string {
    return path.join(config.sessionsDir, token);
  }

  private clearSessionDir(token: string): void {
    fs.rmSync(this.sessionDir(token), { recursive: true, force: true });
  }

  /**
   * Paksa device tampil "offline" ke server WA tanpa memutus koneksi.
   *
   * `markOnlineOnConnect: false` saja tidak cukup: pada pairing baru / setelah
   * reconnect 515, pushName (`creds.me.name`) belum termuat saat connection
   * 'open', sehingga sendPresenceUpdate('unavailable') bawaan Baileys di-skip
   * ("no name present"). Lalu saat name tiba, handler creds.update Baileys
   * mengirim <presence name> TANPA type → server menganggap device 'available'
   * → notif di HP ter-suppress. Re-assert eksplisit 'unavailable' setelah name
   * termuat mengembalikan device ke status offline.
   */
  private async assertOffline(s: Session): Promise<void> {
    try {
      await s.sock?.sendPresenceUpdate('unavailable');
    } catch (err) {
      logger.debug({ err, token: s.token }, 'assertOffline gagal (diabaikan)');
    }
  }

  private startPresenceLoop(s: Session): void {
    this.stopPresenceLoop(s);
    s.presenceTimer = setInterval(() => {
      if (s.status === 'connect') void this.assertOffline(s);
    }, PRESENCE_REASSERT_MS);
  }

  private stopPresenceLoop(s: Session): void {
    if (s.presenceTimer) {
      clearInterval(s.presenceTimer);
      s.presenceTimer = undefined;
    }
  }

  /**
   * Lepas socket lama dengan aman: copot semua listener lalu tutup.
   *
   * Melepas listener LEBIH DULU itu wajib — `end()` memancarkan
   * `connection.update {connection:'close'}`, dan bila handler kita masih
   * terpasang, socket yang sedang kita buang justru menjadwalkan reconnect
   * tandingan yang berebut dengan koneksi baru.
   */
  private discardSocket(s: Session): void {
    const sock = s.sock;
    s.sock = undefined;
    // QR milik socket yang dibuang pasti mati (ref sekali pakai + noise key
    // socket itu) — jangan biarkan disajikan ke pemanggil berikutnya.
    s.qr = undefined;
    s.qrAt = undefined;
    if (!sock) return;
    try {
      sock.ev.removeAllListeners('connection.update');
      sock.ev.removeAllListeners('creds.update');
      sock.ev.removeAllListeners('messages.update');
    } catch (err) {
      logger.debug({ err, token: s.token }, 'Lepas listener socket lama gagal');
    }
    try {
      void sock.end?.(undefined);
    } catch (err) {
      logger.debug({ err, token: s.token }, 'Tutup socket lama gagal (diabaikan)');
    }
  }

  private extractPhone(sock?: WASocket): string | undefined {
    const id = sock?.user?.id;
    if (!id) return undefined;
    return id.split(':')[0].split('@')[0].replace(/[^0-9]/g, '');
  }

  /**
   * Mulai / pastikan koneksi untuk token. Idempoten.
   *
   * AKAR INSIDEN 29 Jul 2026 ada di guard baris kedua. Versi lama berbunyi:
   *
   *     if (s.sock && s.status !== 'disconnect') return s;
   *
   * Handler close men-set `status='connecting'` lalu menjadwalkan `start()`.
   * Bila socket BARU sudah terpasang di `s.sock` tapi belum pernah mencapai
   * 'open' — persis yang terjadi saat WA menutup koneksi beruntun
   * (503 → 405 → 405) — maka pada percobaan berikutnya `s.sock` truthy DAN
   * status 'connecting', sehingga `start()` langsung return tanpa melakukan
   * apa pun. Tidak ada socket hidup, tidak ada timer, tidak ada error: sesi
   * diam permanen sampai container di-restart (~4 jam WA mati tanpa satu
   * baris log pun).
   *
   * Sekarang hanya sesi yang benar-benar 'connect' yang dianggap sehat;
   * 'connecting' tanpa kemajuan (SOCKET_ALIVE_MS) tetap ditutup paksa lalu
   * dibangun ulang — sedangkan 'connecting' yang masih menunjukkan kemajuan
   * (mis. sedang menunggu scan QR) dibiarkan hidup. Lihat guard di bawah.
   */
  async start(token: string): Promise<Session> {
    const s = this.getOrInit(token);
    if (s.starting) return s;
    if (s.sock && s.status === 'connect') return s;

    // Socket 'connecting' yang masih menunjukkan kemajuan adalah HIDUP, bukan
    // zombie — khususnya sesi tanpa creds yang sedang MENUNGGU SCAN QR (Baileys
    // emit & merotasi QR tiap ~20 dtk; status tetap 'connecting' sampai scan
    // sukses). Insiden "Invalid QR code" 24 Sep 2026: dulu start() membuang
    // semua socket 'connecting' apa pun kondisinya, dan getQr() memanggil
    // start() — tiap poll /qr dari UI membunuh socket penerbit QR lalu
    // mengembalikan QR milik socket yang baru saja mati → HP selalu menolak.
    // Bila benar-benar macet (tanpa event, tanpa rotasi QR), jendela
    // SOCKET_ALIVE_MS lewat → socket di-reap seperti semula, dan watchdog
    // tetap menutup kasus sesi ber-creds.
    if (
      s.sock &&
      s.status === 'connecting' &&
      Date.now() - s.lastProgressAt < SOCKET_ALIVE_MS
    ) {
      return s;
    }

    // Sesi 'connecting' yang belum pernah 'open' → socket zombie. Tutup dulu,
    // kalau tidak ia bisa memancarkan connection.update yang menjadwalkan
    // reconnect tandingan dan menimpa state socket yang baru.
    if (s.sock) this.discardSocket(s);

    // Ada start() eksplisit → batalkan reconnect terjadwal supaya tidak dobel.
    this.clearReconnectTimer(s);

    s.starting = true;
    s.manualLogout = false;
    this.markProgress(s);

    const timeout = new Promise<never>((_, reject) => {
      setTimeout(
        () => reject(new Error(`connect timeout ${CONNECT_TIMEOUT_MS}ms`)),
        CONNECT_TIMEOUT_MS,
      ).unref?.();
    });

    try {
      await Promise.race([this.connect(s), timeout]);
    } catch (err) {
      logger.error({ err, token: s.token }, 'Percobaan connect gagal');
      // Jadwalkan ulang; tanpa ini sesi bisa diam tanpa siapa pun mencoba lagi.
      this.discardSocket(s);
      s.status = 'disconnect';
      this.scheduleReconnect(s, undefined);
    } finally {
      s.starting = false;
    }
    return s;
  }

  private async connect(s: Session): Promise<void> {
    const dir = this.sessionDir(s.token);
    fs.mkdirSync(dir, { recursive: true });
    const { state, saveCreds } = await useMultiFileAuthState(dir);
    // Versi di-resolve lewat helper ber-timeout + cache; `undefined` → Baileys
    // pakai versi bawaannya. JANGAN panggil fetchLatestBaileysVersion() langsung
    // di sini: fetch-nya tanpa timeout dan bisa menggantung selamanya.
    const version = await getWaVersion();
    const record = registry.getByToken(s.token);

    const sock = makeWASocket({
      ...(version ? { version } : {}),
      auth: state,
      logger: baileysLogger,
      markOnlineOnConnect: false,
      browser: Browsers.ubuntu(record?.name || 'Althea'),
      syncFullHistory: false,
      generateHighQualityLinkPreview: false,
    });

    s.sock = sock;
    s.status = 'connecting';
    s.qrSeq = 0; // QR counter per-socket — QR pertama socket hidup 60 dtk, rotasi 20 dtk

    sock.ev.on('creds.update', saveCreds);
    sock.ev.on('connection.update', (u) => {
      void this.onConnectionUpdate(s, u);
    });
    // Delivery/read receipt untuk pesan keluar → emit webhook ke api-gateway
    // supaya status clinic_wa_log terupdate: terkirim → sampai → dibaca.
    sock.ev.on('messages.update', (updates) => this.onMessagesUpdate(s, updates));
  }

  /** Map status pesan keluar Baileys → event webhook. fromMe only. */
  private onMessagesUpdate(
    s: Session,
    updates: Array<{ key: { id?: string | null; remoteJid?: string | null; fromMe?: boolean | null }; update: { status?: number | null } }>,
  ): void {
    for (const u of updates) {
      if (!u.key?.fromMe) continue;
      const code = u.update?.status;
      if (typeof code !== 'number') continue;
      // SERVER_ACK=2, DELIVERY_ACK=3, READ=4, PLAYED=5
      const status =
        code >= 4 ? 'read' : code === 3 ? 'delivered' : code === 2 ? 'sent' : undefined;
      if (!status) continue;
      const sender = (u.key.remoteJid ?? '').split('@')[0].replace(/[^0-9]/g, '');
      void emitWebhook({ id: u.key.id ?? undefined, sender, status, device: s.phone });
    }
  }

  private async onConnectionUpdate(s: Session, update: Partial<ConnectionState>): Promise<void> {
    const { connection, lastDisconnect, qr } = update;

    if (qr) {
      try {
        s.qr = await QRCode.toDataURL(qr);
        s.qrAt = Date.now();
        s.qrSeq = (s.qrSeq ?? 0) + 1;
        s.status = 'connecting';
        // Rotasi QR = bukti socket hidup (watchdog & start() jangan menganggap
        // sesi macet saat menunggu scan yang bisa berlangsung bermenit-menit).
        this.markProgress(s);
        // Server WA terjangkau & handshake sukses sampai tahap QR — siklus close
        // "refs attempts ended" (timedOut ~428 setelah beberapa rotasi tanpa
        // scan) bukan kegagalan infrastruktur; jangan biarkan backoff membesar
        // karena itu, supaya pairing ulang tetap responsif.
        s.reconnectAttempts = 0;
        logger.info({ token: s.token }, 'QR baru tersedia');
      } catch (err) {
        logger.error({ err, token: s.token }, 'Gagal render QR ke PNG');
      }
    }

    if (connection === 'open') {
      s.status = 'connect';
      s.qr = undefined;
      s.phone = this.extractPhone(s.sock);
      s.reconnectAttempts = 0; // sukses → backoff kembali ke delay dasar
      this.clearReconnectTimer(s);
      this.markProgress(s);
      logger.info({ token: s.token, phone: s.phone }, 'Device connect');
      // Re-assert 'unavailable' SETELAH name termuat (handler creds.update
      // Baileys baru mengirim presence-with-name beberapa saat setelah 'open').
      // Delay menempatkan unavailable kita sebagai presence terakhir → device
      // tetap offline → notif HP tetap nyala. Lalu jaga via loop berkala.
      setTimeout(() => void this.assertOffline(s), 1500);
      this.startPresenceLoop(s);
    }

    if (connection === 'close') {
      const code = (lastDisconnect?.error as { output?: { statusCode?: number } } | undefined)
        ?.output?.statusCode;
      this.stopPresenceLoop(s);
      this.markProgress(s);

      // 401/403/419 → kredensial mati; reconnect otomatis percuma.
      if (isFatalDisconnect(code) || s.manualLogout) {
        s.status = 'disconnect';
        this.discardSocket(s);
        s.qr = undefined;
        s.reconnectAttempts = 0;
        this.clearReconnectTimer(s);
        this.clearSessionDir(s.token); // creds invalid → bersihkan agar bisa pair akun baru
        logger.warn({ token: s.token, code }, 'Device logged out — perlu pairing ulang');
        return;
      }

      // Penyebab lain (515 restartRequired, 408 timeout, 503, 405, dll) → reconnect.
      // WAJIB lepas socket mati dulu: `start()` menganggap sesi sehat hanya bila
      // status 'connect', tapi socket zombie yang masih memegang listener bisa
      // memancarkan close susulan dan menjadwalkan reconnect tandingan.
      this.discardSocket(s);
      s.status = 'connecting';
      this.scheduleReconnect(s, code);
    }
  }

  private clearReconnectTimer(s: Session): void {
    if (s.reconnectTimer) {
      clearTimeout(s.reconnectTimer);
      s.reconnectTimer = undefined;
    }
  }

  /**
   * Jadwalkan reconnect dengan backoff eksponensial + jitter. Selalu menimpa
   * timer sebelumnya supaya tidak ada dua percobaan paralel untuk satu sesi.
   */
  private scheduleReconnect(s: Session, code: number | undefined): void {
    this.clearReconnectTimer(s);
    s.reconnectAttempts += 1;
    const delayMs = reconnectDelayMs(s.reconnectAttempts);
    logger.warn(
      { token: s.token, code, attempt: s.reconnectAttempts, delayMs },
      'Koneksi tertutup, jadwalkan reconnect',
    );
    s.reconnectTimer = setTimeout(() => {
      s.reconnectTimer = undefined;
      this.start(s.token).catch((err) =>
        logger.error({ err, token: s.token }, 'Reconnect gagal'),
      );
    }, delayMs);
  }

  /**
   * Ambil QR untuk pairing. Bila device sudah connect → alreadyConnected.
   *
   * HANYA menyajikan QR yang masih hidup (umur < ambang per-urutan, lihat
   * QR_FIRST_MAX_AGE_MS / QR_ROTATED_MAX_AGE_MS). QR Baileys memuat ref sekali
   * pakai yang mati saat dirotasi (QR pertama 60 dtk, rotasi 20 dtk) atau saat
   * socket penerbitnya mati — memindai QR basi membuat HP menjawab
   * "Invalid QR code" (insiden 24 Sep 2026: getQr lama langsung mengembalikan
   * `s.qr` apa pun umurnya, termasuk QR milik socket yang baru saja dibuang
   * start()).
   *
   * Tunggu hingga QR_WAIT_TIMEOUT_MS; bila belum dapat → tanpa QR (pemanggil
   * UI retry di tick berikutnya). QR yang layak selalu tersedia dalam hitungan
   * detik selama socket hidup: rotasi berikutnya maksimal 10-20 dtk lagi.
   */
  async getQr(token: string): Promise<{ qrUrl?: string; alreadyConnected: boolean }> {
    const s = this.getOrInit(token);
    if (s.status === 'connect') return { alreadyConnected: true };

    await this.start(token);

    // s.status/s.qr dimutasi handler connection.update (async) — bungkus dalam
    // fungsi supaya TS tidak narrow tipenya berdasarkan control-flow linear.
    const isConnected = () => s.status === 'connect';
    const serveableQr = () => {
      if (!s.qr || s.qrAt === undefined) return undefined;
      const maxAge = (s.qrSeq ?? 0) <= 1 ? QR_FIRST_MAX_AGE_MS : QR_ROTATED_MAX_AGE_MS;
      return Date.now() - s.qrAt < maxAge ? s.qr : undefined;
    };

    const deadline = Date.now() + QR_WAIT_TIMEOUT_MS;
    while (!serveableQr() && !isConnected() && Date.now() < deadline) {
      // Socket bisa mati di tengah tunggu (mis. refs habis → close → reconnect
      // terjadwal). Bila belum ada socket hidup & tidak ada reconnect terjadwal,
      // dorong start() sekarang — admin sedang menunggu di depan layar.
      if (!s.sock && !s.starting && !s.reconnectTimer) {
        await this.start(token);
      }
      await delay(300);
    }
    if (isConnected()) return { alreadyConnected: true };

    return { qrUrl: serveableQr(), alreadyConnected: false };
  }

  /** Status device (untuk endpoint /device & /get-devices). */
  getStatus(token: string): { connected: boolean; phone?: string; name?: string } {
    const s = this.sessions.get(token);
    const record = registry.getByToken(token);
    return {
      connected: s?.status === 'connect',
      phone: s?.phone || record?.phone,
      name: record?.name,
    };
  }

  /**
   * Kirim pesan teks. Tidak mengubah presence / tidak read receipt → notif HP
   * tetap aman. Lempar Error bila device belum connect / gagal kirim.
   */
  sendText(token: string, target: string, message: string): Promise<{ id: string }> {
    return sendTextMessage(this.sessions.get(token), target, message);
  }

  /**
   * Kirim dokumen (PDF/file) sebagai lampiran WA + caption opsional.
   * `data` = Buffer isi file. Dipakai untuk lampiran invoice / bukti pembayaran.
   * Tidak mengubah presence (notif HP tetap aman). Lempar Error bila belum connect.
   */
  sendDocument(
    token: string,
    target: string,
    data: Buffer,
    fileName: string,
    mimetype: string,
    caption?: string,
  ): Promise<{ id: string }> {
    return sendDocumentMessage(
      this.sessions.get(token),
      target,
      data,
      fileName,
      mimetype,
      caption,
    );
  }

  /** Logout device: putuskan + hapus creds supaya bisa scan akun WA lain. */
  async logout(token: string): Promise<void> {
    const s = this.sessions.get(token);
    if (s) {
      s.manualLogout = true;
      this.stopPresenceLoop(s);
      try {
        await s.sock?.logout();
      } catch {
        // socket mungkin sudah mati — abaikan
      }
      s.sock = undefined;
      s.status = 'disconnect';
      s.qr = undefined;
      s.qrAt = undefined;
    }
    this.clearSessionDir(token);
  }

  /** Hapus device sepenuhnya dari memori + creds. */
  async destroy(token: string): Promise<void> {
    await this.logout(token);
    this.sessions.delete(token);
  }

  /** Saat boot: resume semua device yang sudah punya creds tersimpan. */
  resumeAll(): void {
    for (const r of registry.list()) {
      const credsFile = path.join(this.sessionDir(r.token), 'creds.json');
      if (fs.existsSync(credsFile)) {
        logger.info({ token: r.token, name: r.name }, 'Resume sesi tersimpan');
        this.start(r.token).catch((err) =>
          logger.error({ err, token: r.token }, 'Resume gagal'),
        );
      }
    }
  }

  /** True bila token punya creds tersimpan → seharusnya hidup tanpa scan QR. */
  hasStoredCreds(token: string): boolean {
    return fs.existsSync(path.join(this.sessionDir(token), 'creds.json'));
  }

  /** Sesi in-memory untuk token (tanpa membuat baru). Dipakai watchdog. */
  peek(token: string): Session | undefined {
    return this.sessions.get(token);
  }

  /** Tandai kemajuan dari luar (watchdog) supaya tidak dianggap macet lagi. */
  touch(s: Session): void {
    this.markProgress(s);
  }

  /** Aktifkan jaring pengaman reconnect. Lihat `session-watchdog.ts`. */
  startWatchdog(): void {
    startWatchdog(this);
  }

  /** Ringkasan sesi untuk endpoint /health. */
  healthSnapshot(): SessionHealth[] {
    const now = Date.now();
    return registry.list().map((r) => {
      const s = this.sessions.get(r.token);
      return {
        token: r.token,
        name: r.name,
        phone: s?.phone || r.phone,
        status: s?.status ?? 'disconnect',
        reconnectAttempts: s?.reconnectAttempts ?? 0,
        idleMs: s ? now - s.lastProgressAt : -1,
      };
    });
  }
}

export const sessionManager = new SessionManager();
