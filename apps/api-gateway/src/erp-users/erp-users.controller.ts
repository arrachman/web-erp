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
import { CreateErpUserDto } from './dto/create-erp-user.dto';
import { QueryErpUserDto } from './dto/query-erp-user.dto';
import { UpdateErpUserDto } from './dto/update-erp-user.dto';
import { ErpUsersService } from './erp-users.service';

@ApiTags('ERP Users')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/users')
export class ErpUsersController {
  constructor(private readonly service: ErpUsersService) {}

  @Post()
  @ApiOperation({ summary: 'Create ERP user' })
  @ApiResponse({ status: 201, description: 'ERP user created' })
  create(@Body() dto: CreateErpUserDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'List ERP users (paginated)' })
  @ApiResponse({ status: 200, description: 'List of ERP users' })
  findAll(@Query() query: QueryErpUserDto) {
    return this.service.findAll(query);
  }

  @Get('online')
  @ApiOperation({ summary: 'List currently active user sessions' })
  findOnline(@Query('window') window?: string) {
    return this.service.findOnline(window ? parseInt(window, 10) : 30);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP user' })
  @ApiResponse({ status: 200, description: 'ERP user detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP user' })
  @ApiResponse({ status: 200, description: 'ERP user updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpUserDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Soft-delete ERP user' })
  @ApiResponse({ status: 200, description: 'ERP user deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }
}
