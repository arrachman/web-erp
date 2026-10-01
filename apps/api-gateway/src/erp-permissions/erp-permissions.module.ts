import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpPermissionsController } from './erp-permissions.controller';
import { ErpPermissionsService } from './erp-permissions.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpPermissionsController],
  providers: [ErpPermissionsService],
  exports: [ErpPermissionsService],
})
export class ErpPermissionsModule {}
