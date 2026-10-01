import { IsString, MinLength } from 'class-validator';
import { ApiProperty } from '@nestjs/swagger';

export class ErpLoginDto {
  @ApiProperty({
    example: 'admin',
    description: 'Username atau email pengguna ERP',
  })
  @IsString()
  login!: string;

  @ApiProperty({
    example: 'P@ssw0rd',
    description: 'Password pengguna ERP',
  })
  @IsString()
  @MinLength(1)
  password!: string;
}
