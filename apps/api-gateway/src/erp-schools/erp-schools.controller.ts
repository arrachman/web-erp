import {
  Body,
  Controller,
  Delete,
  Get,
  Param,
  Patch,
  Post,
  Query,
  Request,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { BulkSchoolIdsDto, BulkSchoolStatusDto } from './dto/bulk-erp-school.dto';
import { CreateErpSchoolDto } from './dto/create-erp-school.dto';
import { QueryErpSchoolDto } from './dto/query-erp-school.dto';
import { UpdateErpSchoolDto } from './dto/update-erp-school.dto';
import { CreateSchoolActivityDto, SchoolContactInputDto } from './dto/school-contact.dto';
import { ErpSchoolRelationsService } from './erp-school.relations.service';
import { ErpSchoolsService } from './erp-schools.service';

@ApiTags('ERP Schools')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/schools')
export class ErpSchoolsController {
  constructor(
    private readonly service: ErpSchoolsService,
    private readonly relations: ErpSchoolRelationsService,
  ) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP school CRM record' })
  @ApiResponse({ status: 201, description: 'ERP school created' })
  create(@Body() dto: CreateErpSchoolDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get paginated ERP school CRM list' })
  @ApiResponse({ status: 200, description: 'List of ERP schools' })
  findAll(@Query() query: QueryErpSchoolDto) {
    return this.service.findAll(query);
  }

  @Get('alerts')
  @ApiOperation({ summary: 'CRM alerts: schools not ordering in BOS year + contracts expiring' })
  @ApiResponse({ status: 200, description: 'School CRM alerts' })
  alerts(@Query('bosYear') bosYear?: string) {
    return this.service.findAlerts(bosYear ? Number(bosYear) : undefined);
  }

  @Patch('bulk/status')
  @ApiOperation({ summary: 'Bulk activate/deactivate schools' })
  bulkStatus(@Body() dto: BulkSchoolStatusDto, @Request() req: any) {
    return this.service.bulkStatus(dto.ids, dto.isActive ?? true, req.user?.id);
  }

  @Delete('bulk')
  @ApiOperation({ summary: 'Bulk delete schools (soft delete)' })
  bulkRemove(@Body() dto: BulkSchoolIdsDto, @Request() req: any) {
    return this.service.bulkRemove(dto.ids, req.user?.id);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP school CRM record with contacts, addresses, order summary' })
  @ApiResponse({ status: 200, description: 'ERP school detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP school CRM record' })
  @ApiResponse({ status: 200, description: 'ERP school updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpSchoolDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP school CRM record (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP school deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }

  // -------------------------------------------------------------------------
  // Sub-resources
  // -------------------------------------------------------------------------

  @Get(':id/order-history')
  @ApiOperation({ summary: 'Unified order history across channels for one school' })
  orderHistory(@Param('id') id: string) {
    return this.service.getOrderHistory(BigInt(id));
  }

  @Get(':id/activities')
  @ApiOperation({ summary: 'School activity log (visits / negotiations / notes)' })
  activities(@Param('id') id: string, @Query('page') page?: string, @Query('limit') limit?: string) {
    return this.relations.findActivities(
      BigInt(id),
      page ? Number(page) : 1,
      limit ? Number(limit) : 20,
    );
  }

  @Post(':id/activities')
  @ApiOperation({ summary: 'Append a school activity' })
  createActivity(@Param('id') id: string, @Body() dto: CreateSchoolActivityDto, @Request() req: any) {
    return this.relations.createActivity(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id/activities/:activityId')
  @ApiOperation({ summary: 'Delete a school activity (soft delete)' })
  removeActivity(
    @Param('id') id: string,
    @Param('activityId') activityId: string,
    @Request() req: any,
  ) {
    return this.relations.removeActivity(BigInt(id), BigInt(activityId), req.user?.id);
  }

  @Post(':id/contacts')
  @ApiOperation({ summary: 'Add a school contact' })
  addContact(@Param('id') id: string, @Body() dto: SchoolContactInputDto, @Request() req: any) {
    return this.relations.addContact(BigInt(id), dto, req.user?.id);
  }

  @Patch(':id/contacts/:contactId')
  @ApiOperation({ summary: 'Update a school contact' })
  updateContact(
    @Param('id') id: string,
    @Param('contactId') contactId: string,
    @Body() dto: SchoolContactInputDto,
    @Request() req: any,
  ) {
    return this.relations.updateContact(BigInt(id), BigInt(contactId), dto, req.user?.id);
  }

  @Delete(':id/contacts/:contactId')
  @ApiOperation({ summary: 'Delete a school contact (soft delete)' })
  removeContact(
    @Param('id') id: string,
    @Param('contactId') contactId: string,
    @Request() req: any,
  ) {
    return this.relations.removeContact(BigInt(id), BigInt(contactId), req.user?.id);
  }
}
