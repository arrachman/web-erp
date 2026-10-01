import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPartnerTypesController } from './erp-partner-types.controller';
import { ErpPartnerTypesService } from './erp-partner-types.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPartnerTypesController],
  providers: [ErpPartnerTypesService],
  exports: [ErpPartnerTypesService],
})
export class ErpPartnerTypesModule {}
