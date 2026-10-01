import { Body, Controller, Get, Put, Request, UseGuards } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { UpdateErpUserPreferencesDto } from './dto/update-erp-user-preferences.dto';
import { ErpUserPreferencesService } from './erp-user-preferences.service';

@ApiTags('ERP User Preferences')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/user-preferences')
export class ErpUserPreferencesController {
  constructor(private readonly service: ErpUserPreferencesService) {}

  @Get('me')
  @ApiOperation({ summary: 'Get current user preferences (null if not yet saved)' })
  @ApiResponse({ status: 200 })
  getMine(@Request() req: any) {
    return this.service.findForUser(req.user?.id);
  }

  @Put('me')
  @ApiOperation({ summary: 'Upsert current user preferences' })
  @ApiResponse({ status: 200 })
  updateMine(@Body() dto: UpdateErpUserPreferencesDto, @Request() req: any) {
    return this.service.upsertForUser(req.user?.id, dto);
  }
}
