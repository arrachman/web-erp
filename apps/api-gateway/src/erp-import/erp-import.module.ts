import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpImportController } from './erp-import.controller';
import { ErpImportService } from './erp-import.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpImportController],
  providers: [ErpImportService],
  exports: [ErpImportService],
})
export class ErpImportModule {}
