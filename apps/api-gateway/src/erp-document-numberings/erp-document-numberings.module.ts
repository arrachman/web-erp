import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpDocumentNumberingsController } from './erp-document-numberings.controller';
import { ErpDocumentNumberingsService } from './erp-document-numberings.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpDocumentNumberingsController],
  providers: [ErpDocumentNumberingsService],
  exports: [ErpDocumentNumberingsService],
})
export class ErpDocumentNumberingsModule {}
