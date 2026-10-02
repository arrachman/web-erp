import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpFinFxRevaluationsController } from './erp-fin-fx-revaluations.controller';
import { ErpFinFxRevaluationsService } from './erp-fin-fx-revaluations.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinFxRevaluationsController],
  providers: [ErpFinFxRevaluationsService],
})
export class ErpFinFxRevaluationsModule {}
