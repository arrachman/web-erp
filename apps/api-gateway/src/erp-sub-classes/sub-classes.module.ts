import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpSubClassesController } from './sub-classes.controller';
import { ErpSubClassesService } from './sub-classes.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpSubClassesController],
  providers: [ErpSubClassesService],
  exports: [ErpSubClassesService],
})
export class ErpSubClassesModule {}
