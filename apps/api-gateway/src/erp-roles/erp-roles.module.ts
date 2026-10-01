import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpRolesController } from './erp-roles.controller';
import { ErpRolesService } from './erp-roles.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpRolesController],
  providers: [ErpRolesService],
  exports: [ErpRolesService],
})
export class ErpRolesModule {}
