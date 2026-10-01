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
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import { ErpSlsArCollectionsService } from './erp-sls-ar-collections.service';
import { CreateArCollectionDto } from './dto/create-ar-collection.dto';
import { UpdateArCollectionDto } from './dto/update-ar-collection.dto';
import { QueryArCollectionDto } from './dto/query-ar-collection.dto';
import { TransitionArCollectionDto } from './dto/transition-ar-collection.dto';

@ApiTags('ERP Sales — AR Collections (IC)')
@ApiBearerAuth('erp-jwt')
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/sls/ar-collections')
export class ErpSlsArCollectionsController {
  constructor(private readonly service: ErpSlsArCollectionsService) {}

  @Post()
  @ApiOperation({ summary: 'Create AR Collection (Penagihan Piutang)' })
  create(@Body() dto: CreateArCollectionDto, @Request() req: any) {
    return this.service.create(dto, req.user?.sub ?? req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List AR Collections with filters and pagination' })
  findAll(@Query() query: QueryArCollectionDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get single AR Collection by id' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update AR Collection (only when DRAFT or REJECTED)' })
  update(@Param('id') id: string, @Body() dto: UpdateArCollectionDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete AR Collection (not allowed when POSTED)' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.sub ?? req.user?.id);
  }

  @Post(':id/transition')
  @ApiOperation({ summary: 'Workflow transition: SUBMIT / APPROVE / REJECT / POST / REOPEN' })
  transition(
    @Param('id') id: string,
    @Body() dto: TransitionArCollectionDto,
    @Request() req: any,
  ) {
    return this.service.transition(BigInt(id), dto, req.user?.sub ?? req.user?.id);
  }
}
