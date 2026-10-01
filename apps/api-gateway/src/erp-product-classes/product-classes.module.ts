import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpProductClassesController } from './product-classes.controller';
import { ErpProductClassesService } from './product-classes.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpProductClassesController],
  providers: [ErpProductClassesService],
  exports: [ErpProductClassesService],
})
export class ErpProductClassesModule {}
