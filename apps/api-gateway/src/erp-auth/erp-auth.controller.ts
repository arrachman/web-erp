import {
  Controller,
  Post,
  Get,
  Body,
  Req,
  Res,
  UseGuards,
  HttpCode,
  HttpStatus,
  UnauthorizedException,
} from '@nestjs/common';
import { Request, Response } from 'express';
import {
  ApiTags,
  ApiOperation,
  ApiResponse,
  ApiBearerAuth,
  ApiBody,
} from '@nestjs/swagger';
import { ErpJwtAuthGuard } from './guards/erp-jwt-auth.guard';
import { ErpAuthService } from './erp-auth.service';
import { ErpLoginDto } from './dto/erp-login.dto';
import { ErpAuthResponseDto } from './dto/erp-auth-response.dto';

@ApiTags('ERP Auth')
@Controller('erp/auth')
export class ErpAuthController {
  constructor(private readonly erpAuthService: ErpAuthService) {}

  @Post('login')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ summary: 'Login pengguna ERP — kembalikan JWT + set cookie erp_token' })
  @ApiBody({ type: ErpLoginDto })
  @ApiResponse({ status: 200, description: 'Login berhasil', type: ErpAuthResponseDto })
  @ApiResponse({ status: 401, description: 'Username/email atau password salah' })
  async login(
    @Body() dto: ErpLoginDto,
    @Req() req: Request,
    @Res({ passthrough: true }) res: Response,
  ) {
    const erpUser = await this.erpAuthService.validateErpUser(dto.login, dto.password);

    if (!erpUser) {
      throw new UnauthorizedException('Username/email atau password tidak valid');
    }

    const result = await this.erpAuthService.login(erpUser, res, {
      ipAddress: (req.headers['x-forwarded-for'] as string) ?? req.ip ?? null,
      userAgent: req.headers['user-agent'] ?? null,
    });

    return {
      success: true,
      data: result,
    };
  }

  @Post('logout')
  @HttpCode(HttpStatus.OK)
  @ApiOperation({ summary: 'Logout pengguna ERP — hapus cookie erp_token' })
  @ApiResponse({ status: 200, description: 'Logout berhasil' })
  logout(@Res({ passthrough: true }) res: Response) {
    this.erpAuthService.logout(res);
    return {
      success: true,
      message: 'Logout berhasil',
    };
  }

  @UseGuards(ErpJwtAuthGuard)
  @Get('me')
  @ApiBearerAuth()
  @ApiOperation({ summary: 'Ambil profil pengguna ERP saat ini' })
  @ApiResponse({ status: 200, description: 'Profil pengguna ERP' })
  @ApiResponse({ status: 401, description: 'Unauthorized' })
  async getMe(@Req() req: Request & { user: { id: string } }) {
    const data = await this.erpAuthService.getMe(req.user.id);
    return {
      success: true,
      data,
    };
  }
}
