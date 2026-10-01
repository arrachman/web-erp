import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpFinGirosController } from './erp-fin-giros.controller';
import { ErpFinGirosService } from './erp-fin-giros.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinGirosController],
  providers: [ErpFinGirosService],
  exports: [ErpFinGirosService],
})
export class ErpFinGirosModule {}
