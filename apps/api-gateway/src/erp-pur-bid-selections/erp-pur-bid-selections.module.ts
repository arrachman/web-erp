import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPurBidSelectionsController } from './erp-pur-bid-selections.controller';
import { ErpPurBidSelectionsService } from './erp-pur-bid-selections.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPurBidSelectionsController],
  providers: [ErpPurBidSelectionsService],
  exports: [ErpPurBidSelectionsService],
})
export class ErpPurBidSelectionsModule {}
