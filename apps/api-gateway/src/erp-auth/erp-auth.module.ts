import { Module } from '@nestjs/common';
import { PassportModule } from '@nestjs/passport';
import { JwtModule } from '@nestjs/jwt';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuthService } from './erp-auth.service';
import { ErpAuthController } from './erp-auth.controller';
import { ErpJwtStrategy } from './strategies/erp-jwt.strategy';

@Module({
  imports: [
    ConfigModule,
    PrismaModule,
    PassportModule,
    JwtModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (configService: ConfigService) => ({
        secret:
          configService.get<string>('JWT_SECRET') || 'super-secret-key-change-in-production',
        signOptions: {
          expiresIn: configService.get<string>('JWT_EXPIRES_IN') || '1d',
        },
      }),
    }),
  ],
  providers: [ErpAuthService, ErpJwtStrategy],
  controllers: [ErpAuthController],
  exports: [ErpAuthService],
})
export class ErpAuthModule {}
