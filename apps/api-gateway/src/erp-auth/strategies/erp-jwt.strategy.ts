import { Injectable } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { PassportStrategy } from '@nestjs/passport';
import { ExtractJwt, Strategy } from 'passport-jwt';

const ERP_COOKIE = 'erp_token';

function extractErpCookieToken(req: any): string | null {
  if (!req) return null;

  // cookie-parser not used; fall back to raw header parsing
  const cookieHeader = typeof req.headers?.cookie === 'string' ? req.headers.cookie : '';
  if (!cookieHeader) return null;

  const part = cookieHeader
    .split(';')
    .map((p: string) => p.trim())
    .find((p: string) => p.startsWith(`${ERP_COOKIE}=`));

  return part ? part.slice(`${ERP_COOKIE}=`.length) || null : null;
}

@Injectable()
export class ErpJwtStrategy extends PassportStrategy(Strategy, 'erp-jwt') {
  constructor(private configService: ConfigService) {
    super({
      jwtFromRequest: ExtractJwt.fromExtractors([
        extractErpCookieToken,
        ExtractJwt.fromAuthHeaderAsBearerToken(),
      ]),
      ignoreExpiration: false,
      secretOrKey:
        configService.get<string>('JWT_SECRET') ?? 'super-secret-key-change-in-production',
      passReqToCallback: false,
    });
  }

  async validate(payload: any) {
    return {
      id: payload.sub,
      email: payload.email,
      username: payload.username,
      erpLevel: payload.erpLevel,
      sid: payload.sid,
    };
  }
}
