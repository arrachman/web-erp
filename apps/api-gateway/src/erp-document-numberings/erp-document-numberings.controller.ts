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
import { CreateErpDocumentNumberingDto } from './dto/create-erp-document-numbering.dto';
import { QueryErpDocumentNumberingDto } from './dto/query-erp-document-numbering.dto';
import { UpdateErpDocumentNumberingDto } from './dto/update-erp-document-numbering.dto';
import { ErpDocumentNumberingsService } from './erp-document-numberings.service';

@ApiTags('ERP Document Numberings')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/document-numberings')
export class ErpDocumentNumberingsController {
  constructor(private readonly service: ErpDocumentNumberingsService) {}

  @Post()
  @ApiOperation({ summary: 'Create document numbering config' })
  @ApiResponse({ status: 201, description: 'Document numbering created' })
  create(@Body() dto: CreateErpDocumentNumberingDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List document numberings' })
  @ApiResponse({ status: 200, description: 'List of document numberings' })
  findAll(@Query() query: QueryErpDocumentNumberingDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one document numbering by ID' })
  @ApiResponse({ status: 200, description: 'Document numbering detail' })
  @ApiResponse({ status: 404, description: 'Not found' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update document numbering' })
  @ApiResponse({ status: 200, description: 'Document numbering updated' })
  update(
    @Param('id') id: string,
    @Body() dto: UpdateErpDocumentNumberingDto,
    @Request() req: any,
  ) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete document numbering' })
  @ApiResponse({ status: 200, description: 'Document numbering deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }

  @Post(':documentCode/next')
  @ApiOperation({ summary: 'Generate next document number for a given document code' })
  @ApiResponse({ status: 201, description: 'Next document number generated' })
  getNextNumber(@Param('documentCode') documentCode: string) {
    return this.service.getNextNumber(documentCode);
  }
}
