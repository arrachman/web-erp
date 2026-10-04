import {
  Body,
  Controller,
  Get,
  Param,
  Post,
  Query,
  Request,
  Res,
  UseGuards,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiTags } from '@nestjs/swagger';
import { Response } from 'express';
import { ErpJwtAuthGuard } from '../erp-auth/guards/erp-jwt-auth.guard';
import {
  GenerateDocumentsDto,
  QueryDocumentPackagesDto,
  RecordAcceptanceDto,
  SignDocumentDto,
} from './dto/document-packages.dto';
import { ErpDocumentPackagesService } from './erp-document-packages.service';

@ApiTags('ERP Document Packages')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/document-packages')
export class ErpDocumentPackagesController {
  constructor(private readonly service: ErpDocumentPackagesService) {}

  @Get()
  @ApiOperation({ summary: 'Archive of generated procurement documents (per school + budget year)' })
  findAll(@Query() query: QueryDocumentPackagesDto) {
    return this.service.findAll(query);
  }

  @Get('availability/:orderId')
  @ApiOperation({ summary: 'Which documents can be generated for an order + latest versions' })
  availability(@Param('orderId') orderId: string) {
    return this.service.availability(BigInt(orderId));
  }

  @Get('delivery-reports')
  @ApiOperation({ summary: 'Delivery reports with BAST acceptance state' })
  deliveryReports(@Query('acceptance') acceptance?: string) {
    return this.service.listDeliveryReports(acceptance);
  }

  @Post('generate')
  @ApiOperation({ summary: 'Generate one document or a whole package PDF from an order chain' })
  generate(@Body() dto: GenerateDocumentsDto, @Request() req: any) {
    return this.service.generate(dto, req.user?.id);
  }

  @Post('delivery-reports/:drId/acceptance')
  @ApiOperation({ summary: 'Record BAST acceptance (receiver, date) on a delivery report' })
  acceptance(@Param('drId') drId: string, @Body() dto: RecordAcceptanceDto) {
    return this.service.recordAcceptance(BigInt(drId), dto);
  }

  @Post(':id/sign')
  @ApiOperation({ summary: 'Mark a generated document as signed (sign audit trail)' })
  sign(@Param('id') id: string, @Body() dto: SignDocumentDto, @Request() req: any) {
    return this.service.sign(BigInt(id), dto, req.user?.id);
  }

  @Get(':id/file')
  @ApiOperation({ summary: 'Download the generated PDF' })
  async file(@Param('id') id: string, @Res() res: Response) {
    const { buffer, fileName, mimeType } = await this.service.getFile(BigInt(id));
    res.setHeader('Content-Type', mimeType);
    res.setHeader('Content-Disposition', `inline; filename="${fileName}"`);
    res.send(buffer);
  }
}
