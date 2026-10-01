import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpClassesController } from './classes.controller';
import { ErpClassesService } from './classes.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpClassesController],
  providers: [ErpClassesService],
  exports: [ErpClassesService],
})
export class ErpClassesModule {}
