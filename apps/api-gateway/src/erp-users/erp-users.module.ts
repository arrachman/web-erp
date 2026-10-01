import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpUsersController } from './erp-users.controller';
import { ErpUsersService } from './erp-users.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpUsersController],
  providers: [ErpUsersService],
  exports: [ErpUsersService],
})
export class ErpUsersModule {}
