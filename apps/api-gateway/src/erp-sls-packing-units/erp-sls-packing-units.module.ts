import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import {
  ErpSlsPackingListUnitsController,
  ErpSlsPackingUnitsController,
} from './erp-sls-packing-units.controller';
import { ErpSlsPackingUnitsService } from './erp-sls-packing-units.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsPackingListUnitsController, ErpSlsPackingUnitsController],
  providers: [ErpSlsPackingUnitsService],
  exports: [ErpSlsPackingUnitsService],
})
export class ErpSlsPackingUnitsModule {}
