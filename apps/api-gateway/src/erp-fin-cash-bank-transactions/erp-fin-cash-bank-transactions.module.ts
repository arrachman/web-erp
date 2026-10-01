import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpRoleDocPoliciesModule } from '../erp-role-doc-policies/erp-role-doc-policies.module';
import { CashBankPostingService } from './cash-bank-posting.service';
import { ErpFinCashBankTransactionsController } from './erp-fin-cash-bank-transactions.controller';
import { ErpFinCashBankTransactionsService } from './erp-fin-cash-bank-transactions.service';

@Module({
  imports: [PrismaModule, ErpRoleDocPoliciesModule],
  controllers: [ErpFinCashBankTransactionsController],
  providers: [ErpFinCashBankTransactionsService, CashBankPostingService],
  exports: [ErpFinCashBankTransactionsService],
})
export class ErpFinCashBankTransactionsModule {}
