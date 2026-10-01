import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpUnitsController } from './erp-units.controller';
import { ErpUnitsService } from './erp-units.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpUnitsController],
  providers: [ErpUnitsService],
  exports: [ErpUnitsService],
})
export class ErpUnitsModule {}
