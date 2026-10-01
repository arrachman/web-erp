import { ApiProperty } from '@nestjs/swagger';

export class ErpAuthUserDto {
  @ApiProperty({ example: '1' })
  id!: string;

  @ApiProperty({ example: 'admin' })
  username!: string;

  @ApiProperty({ example: 'Administrator' })
  name!: string;

  @ApiProperty({ example: 'admin@example.com' })
  email!: string | null;

  @ApiProperty({ example: 'CENTRAL' })
  erpLevel!: string;
}

export class ErpAuthResponseDto {
  @ApiProperty({ example: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...' })
  accessToken!: string;

  @ApiProperty({ type: ErpAuthUserDto })
  user!: ErpAuthUserDto;
}
