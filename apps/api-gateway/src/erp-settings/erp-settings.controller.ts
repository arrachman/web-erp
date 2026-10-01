import { Body, Controller, Get, Param, Patch, Put, Query, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { QueryErpSettingDto } from './dto/query-erp-setting.dto';
import { UpdateErpSettingDto } from './dto/update-erp-setting.dto';
import { UpdateNumberFormatDto } from './dto/update-number-format.dto';
import { UpdateDateFormatDto } from './dto/update-date-format.dto';
import { ErpSettingsService } from './erp-settings.service';

@ApiTags('ERP Settings')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/settings')
export class ErpSettingsController {
  constructor(private readonly service: ErpSettingsService) {}

  @Get()
  @ApiOperation({ summary: 'List all ERP settings' })
  @ApiResponse({ status: 200, description: 'List of ERP settings' })
  findAll(@Query() query: QueryErpSettingDto) {
    return this.service.findAll(query);
  }

  @Get('number-format')
  @ApiOperation({ summary: 'Get global number formatting (thousands/decimal sep + decimals)' })
  @ApiResponse({ status: 200, description: 'Active number format' })
  getNumberFormat() {
    return this.service.getNumberFormat().then((data) => ({ success: true, data }));
  }

  @Put('number-format')
  @ApiOperation({ summary: 'Update global number formatting' })
  @ApiResponse({ status: 200, description: 'Number format updated' })
  updateNumberFormat(@Body() dto: UpdateNumberFormatDto, @Request() req: any) {
    return this.service
      .updateNumberFormat(dto.thousandsSep, dto.decimalSep, dto.decimals, req.user?.id)
      .then((data) => ({ success: true, data }));
  }

  @Get('date-format')
  @ApiOperation({ summary: 'Get global date display format' })
  @ApiResponse({ status: 200, description: 'Active date format' })
  getDateFormat() {
    return this.service.getDateFormat().then((data) => ({ success: true, data }));
  }

  @Put('date-format')
  @ApiOperation({ summary: 'Update global date display format' })
  @ApiResponse({ status: 200, description: 'Date format updated' })
  updateDateFormat(@Body() dto: UpdateDateFormatDto, @Request() req: any) {
    return this.service
      .updateDateFormat(dto.format, req.user?.id)
      .then((data) => ({ success: true, data }));
  }

  @Get(':key')
  @ApiOperation({ summary: 'Get ERP setting by key' })
  @ApiResponse({ status: 200, description: 'ERP setting detail' })
  @ApiResponse({ status: 404, description: 'Setting not found' })
  findOne(@Param('key') key: string) {
    return this.service.findOne(key);
  }

  @Patch(':key')
  @ApiOperation({ summary: 'Update ERP setting by key' })
  @ApiResponse({ status: 200, description: 'ERP setting updated' })
  @ApiResponse({ status: 404, description: 'Setting not found' })
  upsert(@Param('key') key: string, @Body() dto: UpdateErpSettingDto, @Request() req: any) {
    return this.service.upsert(key, dto, req.user?.id);
  }
}
