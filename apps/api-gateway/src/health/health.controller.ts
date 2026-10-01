import { Controller, Get } from '@nestjs/common';
import { ApiOperation, ApiTags } from '@nestjs/swagger';
import { PrismaService } from '../prisma/prisma.service';

@ApiTags('Health')
@Controller('health')
export class HealthController {
  constructor(private readonly prisma: PrismaService) {}

  @Get()
  @ApiOperation({ summary: 'Service liveness check' })
  liveness() {
    return {
      status: 'ok',
      uptime: process.uptime(),
      timestamp: new Date().toISOString(),
    };
  }

  @Get('readiness')
  @ApiOperation({ summary: 'Service readiness check (verify DB)' })
  async readiness() {
    let dbStatus: 'ok' | 'fail' = 'ok';
    let dbError: string | null = null;
    try {
      await this.prisma.$queryRaw`SELECT 1`;
    } catch (err) {
      dbStatus = 'fail';
      dbError = err instanceof Error ? err.message : String(err);
    }
    const ready = dbStatus === 'ok';
    return {
      status: ready ? 'ready' : 'not_ready',
      checks: { database: { status: dbStatus, error: dbError } },
      timestamp: new Date().toISOString(),
    };
  }
}
