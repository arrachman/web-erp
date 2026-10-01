import { Global, Module } from '@nestjs/common';
import { RefCacheService } from './ref-cache.service';

@Global()
@Module({
  providers: [RefCacheService],
  exports: [RefCacheService],
})
export class RefCacheModule {}
