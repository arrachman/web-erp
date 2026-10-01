import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpAuthModule } from '../erp-auth/erp-auth.module';
import { ErpFormFieldsController } from './erp-form-fields.controller';
import { ErpFormFieldsService } from './erp-form-fields.service';

@Module({
  imports: [PrismaModule, ErpAuthModule],
  controllers: [ErpFormFieldsController],
  providers: [ErpFormFieldsService],
  exports: [ErpFormFieldsService],
})
export class ErpFormFieldsModule {}
