import { BadRequestException, ConflictException } from '@nestjs/common';

type DuplicateExceptionType = 'bad_request' | 'conflict';

type ThrowDuplicateOptions = {
  fieldLabel: string;
  value?: string;
  isSoftDeleted?: boolean;
  type?: DuplicateExceptionType;
};

export function duplicateMessage(
  fieldLabel: string,
  value?: string,
  isSoftDeleted = false,
): string {
  const normalizedValue = typeof value === 'string' ? value.trim() : '';
  const hasValue = normalizedValue.length > 0;
  if (isSoftDeleted) {
    return hasValue
      ? `${fieldLabel} '${normalizedValue}' has been used before and cannot be reused`
      : `${fieldLabel} has been used before and cannot be reused`;
  }
  return hasValue
    ? `${fieldLabel} '${normalizedValue}' already exists`
    : `${fieldLabel} already exists`;
}

export function throwDuplicate({
  fieldLabel,
  value,
  isSoftDeleted = false,
  type = 'bad_request',
}: ThrowDuplicateOptions): never {
  const message = duplicateMessage(fieldLabel, value, isSoftDeleted);
  if (type === 'conflict') {
    throw new ConflictException(message);
  }
  throw new BadRequestException(message);
}

export function isUniqueViolation(error: unknown, targets: string[]): boolean {
  const maybeError = error as {
    code?: unknown;
    meta?: {
      target?: unknown;
    };
  };

  if (maybeError?.code !== 'P2002') {
    return false;
  }

  const rawTarget = maybeError.meta?.target;
  const target = Array.isArray(rawTarget) ? rawTarget.join(',') : String(rawTarget ?? '');
  return targets.some((item) => target.includes(item));
}
