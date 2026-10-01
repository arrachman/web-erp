import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { ConfigService } from '@nestjs/config';
import { ValidationPipe } from '@nestjs/common';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import helmet from 'helmet';
import { BigIntSerializerInterceptor } from './common/interceptors/bigint-serializer.interceptor';
import { AllExceptionsFilter } from './common/filters/all-exceptions.filter';
import { initSentry } from './common/sentry';
import { loadVaultSecrets } from './config/vault';

async function bootstrap() {
  await loadVaultSecrets();
  initSentry(); // must run before NestFactory to instrument http
  const app = await NestFactory.create(AppModule);
  const configService = app.get(ConfigService);

  // Security headers
  app.use(
    helmet({
      contentSecurityPolicy: configService.get('NODE_ENV') === 'production' ? undefined : false,
      crossOriginResourcePolicy: { policy: 'cross-origin' },
    }),
  );

  // Enable CORS — credentials=true requires explicit origin (browser rejects wildcard)
  const corsOriginEnv = configService.get<string>('CORS_ORIGIN', '');
  const explicitOrigins = corsOriginEnv
    .split(',')
    .map((o) => o.trim())
    .filter(Boolean);
  app.enableCors({
    origin: (origin, cb) => {
      // Allow no-origin requests (curl, server-to-server, healthchecks)
      if (!origin) return cb(null, true);
      // Explicit allowlist via env
      if (explicitOrigins.length > 0 && explicitOrigins.includes(origin)) {
        return cb(null, true);
      }
      // Dev defaults: localhost + LAN 192.168.x.x + prod domains
      const devAllowed =
        /^https?:\/\/(localhost|127\.0\.0\.1|0\.0\.0\.0|192\.168\.\d+\.\d+|10\.\d+\.\d+\.\d+|172\.(1[6-9]|2\d|3[01])\.\d+\.\d+)(:\d+)?$/.test(
          origin,
        );
      const prodAllowed = /^https?:\/\/([a-z0-9-]+\.)*fr-labs\.my\.id(:\d+)?$/.test(origin);
      if (devAllowed || prodAllowed) return cb(null, true);
      return cb(new Error(`CORS: origin ${origin} not allowed`), false);
    },
    credentials: true,
    methods: ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization', 'Accept'],
  });

  // Global validation pipe
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,
      transform: true,
      forbidNonWhitelisted: true,
    }),
  );
  app.useGlobalInterceptors(new BigIntSerializerInterceptor());
  app.useGlobalFilters(new AllExceptionsFilter());

  // Global prefix
  app.setGlobalPrefix('api');

  // Swagger Setup
  const config = new DocumentBuilder()
    .setTitle('Sentient Factory API')
    .setDescription('The API Gateway for Sentient Factory')
    .setVersion('1.0')
    .addBearerAuth()
    .build();
  const document = SwaggerModule.createDocument(app, config);
  SwaggerModule.setup('api/docs', app, document);

  const port = configService.get('PORT', 3103);
  await app.listen(port);
  console.log(`API Gateway running on port ${port}`);
  console.log(`Swagger Docs available at http://localhost:${port}/api/docs`);
}
bootstrap();
