import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { PurReturnPostingService } from './pur-return-posting.service';
import { ErpPurReturnsController } from './erp-pur-returns.controller';
import { ErpPurReturnsService } from './erp-pur-returns.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurReturnsController],
  providers: [ErpPurReturnsService, PurReturnPostingService],
  exports: [ErpPurReturnsService],
})
export class ErpPurReturnsModule {}
