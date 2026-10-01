import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { InvDailyCheckPostingService } from './inv-daily-check-posting.service';
import { ErpInvDailyChecksController } from './erp-inv-daily-checks.controller';
import { ErpInvDailyChecksService } from './erp-inv-daily-checks.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpInvDailyChecksController],
  providers: [ErpInvDailyChecksService, InvDailyCheckPostingService],
  exports: [ErpInvDailyChecksService],
})
export class ErpInvDailyChecksModule {}
