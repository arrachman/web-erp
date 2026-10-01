import {
  ArgumentsHost,
  Catch,
  ExceptionFilter,
  HttpException,
  HttpStatus,
  Logger,
} from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { Request, Response } from 'express';

interface ApiErrorResponse {
  success: false;
  statusCode: number;
  error: string;
  message: string;
  path: string;
  timestamp: string;
  details?: unknown;
}

/**
 * Global exception filter — consistent error format across all endpoints.
 *
 * Output shape:
 *   { success: false, statusCode, error, message, path, timestamp, details? }
 *
 * Maps:
 * - HttpException → use status + message
 * - Prisma errors → 409 (P2002 unique), 404 (P2025 not found), 400 (validation)
 * - Other → 500 internal
 */
@Catch()
export class AllExceptionsFilter implements ExceptionFilter {
  private readonly logger = new Logger(AllExceptionsFilter.name);

  catch(exception: unknown, host: ArgumentsHost) {
    const ctx = host.switchToHttp();
    const response = ctx.getResponse<Response>();
    const request = ctx.getRequest<Request>();

    let statusCode = HttpStatus.INTERNAL_SERVER_ERROR;
    let error = 'Internal Server Error';
    let message = 'An unexpected error occurred';
    let details: unknown = undefined;

    if (exception instanceof HttpException) {
      statusCode = exception.getStatus();
      const res = exception.getResponse();
      if (typeof res === 'string') {
        message = res;
      } else if (typeof res === 'object' && res !== null) {
        const obj = res as Record<string, unknown>;
        message = (obj.message as string) || message;
        error = (obj.error as string) || HttpStatus[statusCode] || error;
        // Pass through extra context (e.g., conflictType, conflictBookingId)
        const { message: _m, error: _e, statusCode: _s, ...rest } = obj;
        if (Object.keys(rest).length > 0) details = rest;
      }
    } else if (exception instanceof Prisma.PrismaClientKnownRequestError) {
      const prismaErr = this.mapPrismaError(exception);
      statusCode = prismaErr.statusCode;
      error = prismaErr.error;
      message = prismaErr.message;
      details = { prismaCode: exception.code, meta: exception.meta };
    } else if (exception instanceof Prisma.PrismaClientValidationError) {
      statusCode = HttpStatus.BAD_REQUEST;
      error = 'Validation Error';
      message = 'Invalid query parameters';
    } else if (exception instanceof Error) {
      message = exception.message;
    }

    const body: ApiErrorResponse = {
      success: false,
      statusCode,
      error,
      message,
      path: request.url,
      timestamp: new Date().toISOString(),
      ...(details ? { details } : {}),
    };

    if (statusCode >= 500) {
      this.logger.error(
        `[${request.method} ${request.url}] ${statusCode} ${message}`,
        exception instanceof Error ? exception.stack : undefined,
      );
    } else {
      this.logger.warn(`[${request.method} ${request.url}] ${statusCode} ${message}`);
    }

    response.status(statusCode).json(body);
  }

  private mapPrismaError(err: Prisma.PrismaClientKnownRequestError): {
    statusCode: number;
    error: string;
    message: string;
  } {
    switch (err.code) {
      case 'P2002':
        return {
          statusCode: HttpStatus.CONFLICT,
          error: 'Conflict',
          message: `Unique constraint violated: ${(err.meta?.target as string[])?.join(', ') || 'field'}`,
        };
      case 'P2025':
        return {
          statusCode: HttpStatus.NOT_FOUND,
          error: 'Not Found',
          message: 'Record not found',
        };
      case 'P2003':
        return {
          statusCode: HttpStatus.BAD_REQUEST,
          error: 'Bad Request',
          message: `Foreign key constraint violated: ${(err.meta?.field_name as string) || 'reference'}`,
        };
      case 'P2014':
        return {
          statusCode: HttpStatus.BAD_REQUEST,
          error: 'Bad Request',
          message: 'Required relation violated',
        };
      default:
        return {
          statusCode: HttpStatus.INTERNAL_SERVER_ERROR,
          error: 'Database Error',
          message: `Prisma error ${err.code}`,
        };
    }
  }
}
