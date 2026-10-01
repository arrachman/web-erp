import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPartnersController } from './erp-partners.controller';
import { ErpPartnersService } from './erp-partners.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPartnersController],
  providers: [ErpPartnersService],
  exports: [ErpPartnersService],
})
export class ErpPartnersModule {}
