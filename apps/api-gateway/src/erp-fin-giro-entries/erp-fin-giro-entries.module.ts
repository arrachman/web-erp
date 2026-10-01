import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import {
  ErpFinGiroEntriesController,
  ErpFinGirosLookupController,
} from './erp-fin-giro-entries.controller';
import { ErpFinGiroEntriesService } from './erp-fin-giro-entries.service';
import { GiroPostingService } from './giro-posting.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinGiroEntriesController, ErpFinGirosLookupController],
  providers: [ErpFinGiroEntriesService, GiroPostingService],
  exports: [ErpFinGiroEntriesService],
})
export class ErpFinGiroEntriesModule {}
