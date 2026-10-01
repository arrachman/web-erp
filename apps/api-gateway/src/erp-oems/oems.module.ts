import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpOemsController } from './oems.controller';
import { ErpOemsService } from './oems.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpOemsController],
  providers: [ErpOemsService],
  exports: [ErpOemsService],
})
export class ErpOemsModule {}
