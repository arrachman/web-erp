import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { PurVendorAdvancePostingService } from './pur-vendor-advance-posting.service';
import { ErpPurVendorAdvancesController } from './erp-pur-vendor-advances.controller';
import { ErpPurVendorAdvancesService } from './erp-pur-vendor-advances.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurVendorAdvancesController],
  providers: [ErpPurVendorAdvancesService, PurVendorAdvancePostingService],
  exports: [ErpPurVendorAdvancesService],
})
export class ErpPurVendorAdvancesModule {}
