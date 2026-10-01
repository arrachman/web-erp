import { applyDecorators } from '@nestjs/common';
import { Transform } from 'class-transformer';
import { IsNumberString, ValidationOptions } from 'class-validator';

/**
 * Money/decimal field that accepts either a JSON number or a numeric string and
 * normalises it to a string for `new Prisma.Decimal(...)`. The frontend grids
 * send amounts/rates as numbers (e.g. `25000`, `1`), so plain `@IsNumberString()`
 * would wrongly reject them. Finite numbers are stringified; everything else is
 * left untouched for `@IsNumberString` to validate (and reject if invalid).
 */
export function IsDecimalString(validationOptions?: ValidationOptions) {
  return applyDecorators(
    Transform(({ value }) =>
      typeof value === 'number' && Number.isFinite(value) ? String(value) : value,
    ),
    IsNumberString({}, validationOptions),
  );
}
