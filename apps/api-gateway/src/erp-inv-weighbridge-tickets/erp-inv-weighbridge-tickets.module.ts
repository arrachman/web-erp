import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { InvWeighbridgeTicketPostingService } from './inv-weighbridge-ticket-posting.service';
import { ErpInvWeighbridgeTicketsController } from './erp-inv-weighbridge-tickets.controller';
import { ErpInvWeighbridgeTicketsService } from './erp-inv-weighbridge-tickets.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpInvWeighbridgeTicketsController],
  providers: [ErpInvWeighbridgeTicketsService, InvWeighbridgeTicketPostingService],
  exports: [ErpInvWeighbridgeTicketsService],
})
export class ErpInvWeighbridgeTicketsModule {}
