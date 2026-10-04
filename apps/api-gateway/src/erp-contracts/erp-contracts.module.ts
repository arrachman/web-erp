import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpContractsController } from './erp-contracts.controller';
import { ErpContractsService } from './erp-contracts.service';
import { ErpBundlesService } from './erp-bundles.service';

/** Fase 3 W7 — harga kontrak per sekolah + paket/bundle. */
@Module({
  imports: [PrismaModule],
  controllers: [ErpContractsController],
  providers: [ErpContractsService, ErpBundlesService],
  exports: [ErpContractsService, ErpBundlesService],
})
export class ErpContractsModule {}
