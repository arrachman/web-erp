import {
  Injectable,
  Logger,
  NotFoundException,
} from '@nestjs/common';
import { randomUUID } from 'crypto';
import { Response } from 'express';
import { JwtService } from '@nestjs/jwt';
import { PrismaService } from '../prisma/prisma.service';
import { verifyPassword } from '../common/auth/password-hasher';
import type { ErpAuthResponseDto } from './dto/erp-auth-response.dto';

const ERP_COOKIE_NAME = 'erp_token';
const ERP_COOKIE_MAX_AGE_MS = 24 * 60 * 60 * 1000; // 1 day

export interface ErpLoginMeta {
  ipAddress?: string | null;
  userAgent?: string | null;
}

@Injectable()
export class ErpAuthService {
  private readonly logger = new Logger(ErpAuthService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly jwtService: JwtService,
  ) {}

  /**
   * Validate ERP user by username OR email (case-insensitive).
   * Returns the user record (without passwordHash) on success, null otherwise.
   */
  async validateErpUser(login: string, password: string) {
    const loginLower = login.toLowerCase().trim();

    const erpUser = await this.prisma.erpUser.findFirst({
      where: {
        deletedAt: null,
        OR: [
          { code: { equals: loginLower, mode: 'insensitive' } },
          { email: { equals: loginLower, mode: 'insensitive' } },
        ],
      },
    });

    if (!erpUser) {
      return null;
    }

    const passwordValid = await verifyPassword(password, erpUser.passwordHash);
    if (!passwordValid) {
      return null;
    }

    if (!erpUser.isActive) {
      return null;
    }

    const { passwordHash: _ph, ...safeUser } = erpUser;
    return safeUser;
  }

  /**
   * Sign JWT, set cookie, and return access token + user info.
   */
  async login(
    erpUser: Awaited<ReturnType<ErpAuthService['validateErpUser']>> & object,
    res: Response,
    meta?: ErpLoginMeta,
  ): Promise<ErpAuthResponseDto> {
    const sid = randomUUID();

    const payload = {
      sub: erpUser.id.toString(),
      email: erpUser.email,
      username: erpUser.code,
      erpLevel: erpUser.level,
      sid,
    };

    const accessToken = this.jwtService.sign(payload);

    res.cookie(ERP_COOKIE_NAME, accessToken, {
      httpOnly: true,
      sameSite: 'lax',
      maxAge: ERP_COOKIE_MAX_AGE_MS,
      path: '/',
    });

    const ipAddress = this.normalizeHeaderValue(meta?.ipAddress);
    const userAgent = this.normalizeHeaderValue(meta?.userAgent);

    this.logger.log(
      `ERP login: user=${erpUser.code} ip=${ipAddress ?? 'unknown'} ua=${userAgent ?? 'unknown'}`,
    );

    return {
      accessToken,
      user: {
        id: erpUser.id.toString(),
        username: erpUser.code,
        name: erpUser.name,
        email: erpUser.email ?? null,
        erpLevel: erpUser.level,
      },
    };
  }

  /**
   * Clear the ERP auth cookie.
   */
  logout(res: Response): void {
    res.clearCookie(ERP_COOKIE_NAME, { path: '/' });
  }

  /**
   * Fetch full ErpUser by id (BigInt), omitting passwordHash.
   */
  async getMe(userId: string) {
    const id = BigInt(userId);

    const erpUser = await this.prisma.erpUser.findFirst({
      where: { id, deletedAt: null },
    });

    if (!erpUser) {
      throw new NotFoundException(`ERP user not found`);
    }

    const { passwordHash: _ph, ...safeUser } = erpUser;
    return {
      ...safeUser,
      id: safeUser.id.toString(),
    };
  }

  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  private normalizeHeaderValue(value: string | null | undefined): string | null {
    if (typeof value !== 'string') {
      return null;
    }
    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
  }
}
