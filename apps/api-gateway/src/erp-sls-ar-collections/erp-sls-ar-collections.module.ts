import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpSlsArCollectionsController } from './erp-sls-ar-collections.controller';
import { ErpSlsArCollectionsService } from './erp-sls-ar-collections.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSlsArCollectionsController],
  providers: [ErpSlsArCollectionsService],
  exports: [ErpSlsArCollectionsService],
})
export class ErpSlsArCollectionsModule {}
