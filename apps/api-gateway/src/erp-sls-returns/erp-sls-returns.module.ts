import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { SlsReturnPostingService } from './sls-return-posting.service';
import { ErpSlsReturnsController } from './erp-sls-returns.controller';
import { ErpSlsReturnsService } from './erp-sls-returns.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsReturnsController],
  providers: [ErpSlsReturnsService, SlsReturnPostingService],
  exports: [ErpSlsReturnsService],
})
export class ErpSlsReturnsModule {}
