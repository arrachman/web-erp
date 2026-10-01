import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpColorsController } from './erp-colors.controller';
import { ErpColorsService } from './erp-colors.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpColorsController],
  providers: [ErpColorsService],
  exports: [ErpColorsService],
})
export class ErpColorsModule {}
