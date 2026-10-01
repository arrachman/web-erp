import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAccountsController } from './erp-accounts.controller';
import { ErpAccountsService } from './erp-accounts.service';
import { ErpAccountsHierarchy } from './erp-accounts.hierarchy';

@Module({
  imports: [PrismaModule],
  controllers: [ErpAccountsController],
  providers: [ErpAccountsService, ErpAccountsHierarchy],
  exports: [ErpAccountsService],
})
export class ErpAccountsModule {}
