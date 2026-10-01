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
import { CreateErpPartnerDto } from './dto/create-erp-partner.dto';
import { QueryErpPartnerDto } from './dto/query-erp-partner.dto';
import { UpdateErpPartnerDto } from './dto/update-erp-partner.dto';
import { CreateErpPartnerAddressDto } from './dto/create-erp-partner-address.dto';
import { UpdateErpPartnerAddressDto } from './dto/update-erp-partner-address.dto';
import { CreateErpPartnerContactDto } from './dto/create-erp-partner-contact.dto';
import { UpdateErpPartnerContactDto } from './dto/update-erp-partner-contact.dto';
import { CreateErpPartnerBankAccountDto } from './dto/create-erp-partner-bank-account.dto';
import { ErpPartnersService } from './erp-partners.service';

@ApiTags('ERP Partners')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/partners')
export class ErpPartnersController {
  constructor(private readonly service: ErpPartnersService) {}

  // ---------------------------------------------------------------------------
  // Partner CRUD
  // ---------------------------------------------------------------------------

  @Post()
  @ApiOperation({ summary: 'Create ERP partner' })
  @ApiResponse({ status: 201, description: 'ERP partner created' })
  create(@Body() dto: CreateErpPartnerDto, @Request() req: any) {
    return this.service.create(dto, req.user?.id);
  }

  @Get()
  @ApiOperation({ summary: 'Get ERP partner list' })
  @ApiResponse({ status: 200, description: 'List of ERP partners' })
  findAll(@Query() query: QueryErpPartnerDto) {
    return this.service.findAll(query);
  }

  @Get(':id')
  @ApiOperation({ summary: 'Get one ERP partner with addresses, contacts, and bank accounts' })
  @ApiResponse({ status: 200, description: 'ERP partner detail' })
  findOne(@Param('id') id: string) {
    return this.service.findOne(BigInt(id));
  }

  @Patch(':id')
  @ApiOperation({ summary: 'Update ERP partner' })
  @ApiResponse({ status: 200, description: 'ERP partner updated' })
  update(@Param('id') id: string, @Body() dto: UpdateErpPartnerDto, @Request() req: any) {
    return this.service.update(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id')
  @ApiOperation({ summary: 'Delete ERP partner (soft delete)' })
  @ApiResponse({ status: 200, description: 'ERP partner deleted' })
  remove(@Param('id') id: string, @Request() req: any) {
    return this.service.remove(BigInt(id), req.user?.id);
  }

  // ---------------------------------------------------------------------------
  // Addresses
  // ---------------------------------------------------------------------------

  @Post(':id/addresses')
  @ApiOperation({ summary: 'Add address to ERP partner' })
  @ApiResponse({ status: 201, description: 'Partner address added' })
  addAddress(
    @Param('id') id: string,
    @Body() dto: CreateErpPartnerAddressDto,
    @Request() req: any,
  ) {
    return this.service.addAddress(BigInt(id), dto, req.user?.id);
  }

  @Patch(':id/addresses/:addressId')
  @ApiOperation({ summary: 'Update address of ERP partner' })
  @ApiResponse({ status: 200, description: 'Partner address updated' })
  updateAddress(
    @Param('addressId') addressId: string,
    @Body() dto: UpdateErpPartnerAddressDto,
    @Request() req: any,
  ) {
    return this.service.updateAddress(BigInt(addressId), dto, req.user?.id);
  }

  @Delete(':id/addresses/:addressId')
  @ApiOperation({ summary: 'Remove address from ERP partner (soft delete)' })
  @ApiResponse({ status: 200, description: 'Partner address deleted' })
  removeAddress(@Param('addressId') addressId: string, @Request() req: any) {
    return this.service.removeAddress(BigInt(addressId), req.user?.id);
  }

  // ---------------------------------------------------------------------------
  // Contacts
  // ---------------------------------------------------------------------------

  @Post(':id/contacts')
  @ApiOperation({ summary: 'Add contact to ERP partner' })
  @ApiResponse({ status: 201, description: 'Partner contact added' })
  addContact(
    @Param('id') id: string,
    @Body() dto: CreateErpPartnerContactDto,
    @Request() req: any,
  ) {
    return this.service.addContact(BigInt(id), dto, req.user?.id);
  }

  @Patch(':id/contacts/:contactId')
  @ApiOperation({ summary: 'Update contact of ERP partner' })
  @ApiResponse({ status: 200, description: 'Partner contact updated' })
  updateContact(
    @Param('contactId') contactId: string,
    @Body() dto: UpdateErpPartnerContactDto,
    @Request() req: any,
  ) {
    return this.service.updateContact(BigInt(contactId), dto, req.user?.id);
  }

  @Delete(':id/contacts/:contactId')
  @ApiOperation({ summary: 'Remove contact from ERP partner (soft delete)' })
  @ApiResponse({ status: 200, description: 'Partner contact deleted' })
  removeContact(@Param('contactId') contactId: string, @Request() req: any) {
    return this.service.removeContact(BigInt(contactId), req.user?.id);
  }

  // ---------------------------------------------------------------------------
  // Bank Accounts
  // ---------------------------------------------------------------------------

  @Post(':id/bank-accounts')
  @ApiOperation({ summary: 'Add bank account to ERP partner' })
  @ApiResponse({ status: 201, description: 'Partner bank account added' })
  addBankAccount(
    @Param('id') id: string,
    @Body() dto: CreateErpPartnerBankAccountDto,
    @Request() req: any,
  ) {
    return this.service.addBankAccount(BigInt(id), dto, req.user?.id);
  }

  @Delete(':id/bank-accounts/:bankId')
  @ApiOperation({ summary: 'Remove bank account from ERP partner (soft delete)' })
  @ApiResponse({ status: 200, description: 'Partner bank account deleted' })
  removeBankAccount(@Param('bankId') bankId: string, @Request() req: any) {
    return this.service.removeBankAccount(BigInt(bankId), req.user?.id);
  }
}
