import { Module } from '@nestjs/common';
import { PrismaModule } from '../prisma/prisma.module';
import { ErpLanguagesController } from './erp-languages.controller';
import { ErpLanguagesService } from './erp-languages.service';

@Module({
  imports: [PrismaModule],
  controllers: [ErpLanguagesController],
  providers: [ErpLanguagesService],
  exports: [ErpLanguagesService],
})
export class ErpLanguagesModule {}
