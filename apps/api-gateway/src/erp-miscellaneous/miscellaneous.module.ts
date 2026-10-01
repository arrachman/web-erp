import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpMiscellaneousController } from './miscellaneous.controller';
import { ErpMiscellaneousService } from './miscellaneous.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpMiscellaneousController],
  providers: [ErpMiscellaneousService],
  exports: [ErpMiscellaneousService],
})
export class ErpMiscellaneousModule {}
