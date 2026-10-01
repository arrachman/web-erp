import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpFinArReceiptsController } from './erp-fin-ar-receipts.controller';
import { ErpFinArReceiptsService } from './erp-fin-ar-receipts.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinArReceiptsController],
  providers: [ErpFinArReceiptsService],
  exports: [ErpFinArReceiptsService],
})
export class ErpFinArReceiptsModule {}
