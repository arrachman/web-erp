import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpBankAccountsController } from './erp-bank-accounts.controller';
import { ErpBankAccountsService } from './erp-bank-accounts.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpBankAccountsController],
  providers: [ErpBankAccountsService],
  exports: [ErpBankAccountsService],
})
export class ErpBankAccountsModule {}
