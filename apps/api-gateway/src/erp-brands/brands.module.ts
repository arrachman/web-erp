import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpBrandsController } from './brands.controller';
import { ErpBrandsService } from './brands.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpBrandsController],
  providers: [ErpBrandsService],
  exports: [ErpBrandsService],
})
export class ErpBrandsModule {}
