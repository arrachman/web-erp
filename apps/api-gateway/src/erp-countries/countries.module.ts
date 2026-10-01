import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpCountriesController } from './countries.controller';
import { ErpCountriesService } from './countries.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpCountriesController],
  providers: [ErpCountriesService],
  exports: [ErpCountriesService],
})
export class ErpCountriesModule {}
