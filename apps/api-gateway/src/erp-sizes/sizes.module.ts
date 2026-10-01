import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpSizesController } from './sizes.controller';
import { ErpSizesService } from './sizes.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpSizesController],
  providers: [ErpSizesService],
  exports: [ErpSizesService],
})
export class ErpSizesModule {}
