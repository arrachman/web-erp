import { Module } from '@nestjs/common';
import { ErpToolsController } from './erp-tools.controller';
import { ErpToolsService } from './erp-tools.service';

@Module({
  controllers: [ErpToolsController],
  providers: [ErpToolsService],
  exports: [ErpToolsService],
})
export class ErpToolsModule {}
