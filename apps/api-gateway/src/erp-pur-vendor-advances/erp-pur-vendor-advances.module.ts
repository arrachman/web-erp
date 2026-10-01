import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPurVendorAdvancesController } from './erp-pur-vendor-advances.controller';
import { ErpPurVendorAdvancesService } from './erp-pur-vendor-advances.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurVendorAdvancesController],
  providers: [ErpPurVendorAdvancesService],
  exports: [ErpPurVendorAdvancesService],
})
export class ErpPurVendorAdvancesModule {}
