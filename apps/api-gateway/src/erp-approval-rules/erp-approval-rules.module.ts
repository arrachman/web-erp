import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpApprovalRulesController } from './erp-approval-rules.controller';
import { ErpApprovalRulesService } from './erp-approval-rules.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpApprovalRulesController],
  providers: [ErpApprovalRulesService],
  exports: [ErpApprovalRulesService],
})
export class ErpApprovalRulesModule {}
