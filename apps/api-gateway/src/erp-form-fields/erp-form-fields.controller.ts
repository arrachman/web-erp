import { Body, Controller, Get, Param, Put, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { SaveFormFieldsDto } from './dto/save-form-fields.dto';
import { ErpFormFieldsService } from './erp-form-fields.service';

@ApiTags('ERP Form Fields (Form Builder)')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/transaction-forms')
export class ErpFormFieldsController {
  constructor(private readonly service: ErpFormFieldsService) {}

  @Get(':code/fields')
  @ApiOperation({ summary: 'Get form field config for a transaction type (lazy-seeds defaults)' })
  getFields(@Param('code') code: string) {
    return this.service.getFields(code);
  }

  @Put(':code/fields')
  @ApiOperation({ summary: 'Replace form field config for a transaction type' })
  saveFields(
    @Param('code') code: string,
    @Body() dto: SaveFormFieldsDto,
    @Request() req: any,
  ) {
    return this.service.saveFields(code, dto, req.user?.id);
  }
}
