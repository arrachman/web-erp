import { Module } from '@nestjs/common';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { JwtModule } from '@nestjs/jwt';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpSlsOrdersModule } from '../erp-sls-orders/erp-sls-orders.module';
import { ErpContractsModule } from '../erp-contracts/erp-contracts.module';
import {
  ErpPortalAdminController,
  ErpPortalController,
  ErpPortalPublicController,
} from './erp-portal.controller';
import { ErpPortalAccountsService } from './erp-portal-accounts.service';
import { ErpPortalShopService } from './erp-portal-shop.service';
import { ErpPortalAuthGuard } from './erp-portal.guard';

@Module({
  imports: [
    PrismaModule,
    ErpSlsOrdersModule,
    ErpContractsModule,
    JwtModule.registerAsync({
      imports: [ConfigModule],
      inject: [ConfigService],
      useFactory: (configService: ConfigService) => ({
        secret:
          configService.get<string>('JWT_SECRET') ||
          'super-secret-key-change-in-production',
        signOptions: { expiresIn: '7d' },
      }),
    }),
  ],
  controllers: [
    ErpPortalPublicController,
    ErpPortalController,
    ErpPortalAdminController,
  ],
  providers: [ErpPortalAccountsService, ErpPortalShopService, ErpPortalAuthGuard],
  exports: [ErpPortalAccountsService, ErpPortalShopService],
})
export class ErpPortalModule {}
