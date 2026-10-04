import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpDocumentPackagesController } from './erp-document-packages.controller';
import { ErpDocumentPackagesService } from './erp-document-packages.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpDocumentPackagesController],
  providers: [ErpDocumentPackagesService],
  exports: [ErpDocumentPackagesService],
})
export class ErpDocumentPackagesModule {}
