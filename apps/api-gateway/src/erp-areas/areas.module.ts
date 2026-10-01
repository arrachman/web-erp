import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpAreasController } from './areas.controller';
import { ErpAreasService } from './areas.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpAreasController],
  providers: [ErpAreasService],
  exports: [ErpAreasService],
})
export class ErpAreasModule {}
