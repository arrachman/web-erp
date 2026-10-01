import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpSysMenusController } from './erp-sys-menus.controller';
import { ErpSysMenusService } from './erp-sys-menus.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpSysMenusController],
  providers: [ErpSysMenusService],
  exports: [ErpSysMenusService],
})
export class ErpSysMenusModule {}
