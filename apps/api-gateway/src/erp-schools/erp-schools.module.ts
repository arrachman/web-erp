import { Module } from '@nestjs/common';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpSchoolRelationsService } from './erp-school.relations.service';
import { ErpSchoolsController } from './erp-schools.controller';
import { ErpSchoolsService } from './erp-schools.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpSchoolsController],
  providers: [ErpSchoolsService, ErpSchoolRelationsService],
})
export class ErpSchoolsModule {}
