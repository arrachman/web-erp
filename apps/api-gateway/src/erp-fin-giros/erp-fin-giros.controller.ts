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
import { CreateGiroDto } from './dto/create-giro.dto';
import { QueryGiroDto } from './dto/query-giro.dto';
import { UpdateGiroDto } from './dto/update-giro.dto';
import { ErpFinGirosService } from './erp-fin-giros.service';

@ApiTags('ERP Fin Giros')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/fin/giros')
export class ErpFinGirosController {
  constructor(private readonly service: ErpFinGirosService) {}

  @Post()
  @ApiOperation({ summary: 'Create giro' })
  create(@Body() dto: CreateGiroDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List giros' })
  findAll(@Query() query: QueryGiroDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get giro' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update giro' })
  update(
    @Param('id') id: string,
    @Body() dto: UpdateGiroDto,
    @Request() req: any,
  ) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete giro' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
