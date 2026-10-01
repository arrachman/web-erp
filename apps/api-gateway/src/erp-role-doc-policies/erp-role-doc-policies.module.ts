import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpRoleDocPoliciesController } from './erp-role-doc-policies.controller';
import { ErpRoleDocPoliciesService } from './erp-role-doc-policies.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpRoleDocPoliciesController],
  providers: [ErpRoleDocPoliciesService],
  exports: [ErpRoleDocPoliciesService],
})
export class ErpRoleDocPoliciesModule {}
