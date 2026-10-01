import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpSubAreasController } from './sub-areas.controller';
import { ErpSubAreasService } from './sub-areas.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpSubAreasController],
  providers: [ErpSubAreasService],
  exports: [ErpSubAreasService],
})
export class ErpSubAreasModule {}
