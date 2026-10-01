import { ApiPropertyOptional } from '@nestjs/swagger';
import { Type } from 'class-transformer';
import { IsEnum, IsInt, IsOptional, IsString, Max, Min } from 'class-validator';
import { ErpGiroStatusDto, ErpGiroTypeDto } from './create-giro.dto';

export class QueryGiroDto {
  @ApiPropertyOptional({ default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({ default: 10 })
  @IsOptional()
  @Type(() => Number)
  @IsInt()
  @Min(1)
  @Max(100)
  limit?: number = 10;

  @ApiPropertyOptional()
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({ enum: ErpGiroTypeDto })
  @IsOptional()
  @IsEnum(ErpGiroTypeDto)
  type?: ErpGiroTypeDto;

  @ApiPropertyOptional({ enum: ErpGiroStatusDto })
  @IsOptional()
  @IsEnum(ErpGiroStatusDto)
  status?: ErpGiroStatusDto;
}
