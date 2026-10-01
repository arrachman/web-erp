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
import { CreateErpNotificationDto } from './dto/create-notification.dto';
import { QueryErpNotificationsDto } from './dto/query-notifications.dto';
import { ErpNotificationsService } from './erp-notifications.service';

@ApiTags('ERP Notifications')
@ApiBearerAuth()
@UseGuards(ErpJwtAuthGuard)
@Controller('erp/notifications')
export class ErpNotificationsController {
  constructor(private readonly service: ErpNotificationsService) {}

  @Get('me')
  @ApiOperation({ summary: 'List notifikasi user aktif (paginated)' })
  listMine(@Request() req: any, @Query() q: QueryErpNotificationsDto) {
    return this.service.listForUser(req.user?.id, q);
  }

  @Get('me/unread-count')
  @ApiOperation({ summary: 'Hitung notifikasi belum dibaca' })
  countUnread(@Request() req: any) {
    return this.service.unreadCount(req.user?.id);
  }

  @Patch('me/:id/read')
  @ApiOperation({ summary: 'Tandai satu notifikasi sebagai dibaca' })
  markRead(@Request() req: any, @Param('id') id: string) {
    return this.service.markRead(req.user?.id, id);
  }

  @Post('me/read-all')
  @ApiOperation({ summary: 'Tandai semua notifikasi user sebagai dibaca' })
  markAllRead(@Request() req: any) {
    return this.service.markAllRead(req.user?.id);
  }

  @Delete('me/:id')
  @ApiOperation({ summary: 'Arsipkan notifikasi (soft hide)' })
  archive(@Request() req: any, @Param('id') id: string) {
    return this.service.archive(req.user?.id, id);
  }

  @Post()
  @ApiOperation({ summary: 'Buat notifikasi baru (internal/admin)' })
  create(@Body() dto: CreateErpNotificationDto) {
    return this.service.create(dto);
  }
}
