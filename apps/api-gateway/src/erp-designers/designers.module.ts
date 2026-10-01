import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpDesignersController } from './designers.controller';
import { ErpDesignersService } from './designers.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpDesignersController],
  providers: [ErpDesignersService],
  exports: [ErpDesignersService],
})
export class ErpDesignersModule {}
