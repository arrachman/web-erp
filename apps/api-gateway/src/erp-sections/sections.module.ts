import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuditModule } from '../erp-audit/erp-audit.module';
import { ErpSectionsController } from './sections.controller';
import { ErpSectionsService } from './sections.service';

@Module({
  imports: [PrismaModule, ErpAuditModule],
  controllers: [ErpSectionsController],
  providers: [ErpSectionsService],
  exports: [ErpSectionsService],
})
export class ErpSectionsModule {}
