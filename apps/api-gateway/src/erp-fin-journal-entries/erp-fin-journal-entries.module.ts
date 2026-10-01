import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpFinJournalEntriesController } from './erp-fin-journal-entries.controller';
import { ErpFinJournalEntriesService } from './erp-fin-journal-entries.service';
import { JournalPostingService } from './journal-posting.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpFinJournalEntriesController],
  providers: [ErpFinJournalEntriesService, JournalPostingService],
  exports: [ErpFinJournalEntriesService],
})
export class ErpFinJournalEntriesModule {}
