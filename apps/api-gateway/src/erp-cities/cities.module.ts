import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpCitiesController } from './cities.controller';
import { ErpCitiesService } from './cities.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpCitiesController],
  providers: [ErpCitiesService],
  exports: [ErpCitiesService],
})
export class ErpCitiesModule {}
