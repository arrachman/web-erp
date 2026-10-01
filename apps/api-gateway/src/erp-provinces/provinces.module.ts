import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpProvincesController } from './provinces.controller';
import { ErpProvincesService } from './provinces.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpProvincesController],
  providers: [ErpProvincesService],
  exports: [ErpProvincesService],
})
export class ErpProvincesModule {}
