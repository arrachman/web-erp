import { IsIn, IsObject, IsOptional } from 'class-validator';

export class RenderRegistryReportDto {
  @IsOptional()
  @IsObject()
  params?: Record<string, unknown>;

  @IsIn(['html', 'pdf', 'docx', 'xlsx'])
  format!: 'html' | 'pdf' | 'docx' | 'xlsx';

  @IsOptional()
  @IsIn(['layout', 'data'])
  mode?: 'layout' | 'data';
}
