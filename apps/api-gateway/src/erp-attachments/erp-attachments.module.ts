import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpTransactionAttachmentsController } from './erp-transaction-attachments.controller';
import { ErpTransactionAttachmentsService } from './erp-transaction-attachments.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpTransactionAttachmentsController],
  providers: [ErpTransactionAttachmentsService],
  exports: [ErpTransactionAttachmentsService],
})
export class ErpAttachmentsModule {}
