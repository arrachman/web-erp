import { Injectable } from '@nestjs/common';
import { AuthGuard } from '@nestjs/passport';

@Injectable()
export class ErpJwtAuthGuard extends AuthGuard('erp-jwt') {}
