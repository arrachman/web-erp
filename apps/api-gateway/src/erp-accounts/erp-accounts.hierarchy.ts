import { BadRequestException, Injectable } from '@nestjs/common';
import { ErpAccountType } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import {
  accountKindFromCode,
  isLeafAccountCode,
  normalBalanceForAccountType,
} from './account-hierarchy';
import type { AccountCodeFormat } from './account-code-format';

const MAX_CYCLE_DEPTH = 50;

interface AccountParent {
  id: bigint;
  type: ErpAccountType;
  kind: 'HEADER' | 'POSTABLE';
  level: number;
}

@Injectable()
export class ErpAccountsHierarchy {
  constructor(private prisma: PrismaService) {}

  async loadParent(parentId: bigint): Promise<AccountParent> {
    const parent = await this.prisma.erpAccount.findFirst({
      where: { id: BigInt(parentId), deletedAt: null },
      select: { id: true, type: true, kind: true, level: true },
    });
    if (!parent) {
      throw new BadRequestException('Parent account tidak ditemukan');
    }
    if (parent.kind !== 'HEADER') {
      throw new BadRequestException(
        'Parent harus berupa akun HEADER; akun POSTABLE tidak bisa memiliki anak',
      );
    }
    return parent;
  }

  async assertNotSelfOrDescendant(accountId: bigint, parentCandidateId: bigint): Promise<void> {
    if (parentCandidateId === accountId) {
      throw new BadRequestException('Akun tidak bisa menjadi parent dari dirinya sendiri');
    }
    let cursor: bigint | null = BigInt(parentCandidateId);
    for (let i = 0; i < MAX_CYCLE_DEPTH && cursor; i++) {
      const node: { id: bigint; parentId: bigint | null } | null =
        await this.prisma.erpAccount.findFirst({
          where: { id: cursor, deletedAt: null },
          select: { parentId: true, id: true },
        });
      if (!node) break;
      if (node.parentId === null) break;
      if (node.parentId === accountId) {
        throw new BadRequestException(
          'Parent tidak boleh merupakan keturunan dari akun ini (membentuk siklus)',
        );
      }
      cursor = node.parentId;
    }
  }

  async countChildren(accountId: bigint): Promise<number> {
    return this.prisma.erpAccount.count({
      where: { parentId: BigInt(accountId), deletedAt: null },
    });
  }

  async validateCurrency(currencyId: bigint): Promise<void> {
    const currency = await this.prisma.erpCurrency.findFirst({
      where: { id: BigInt(currencyId), deletedAt: null, isActive: true },
      select: { id: true },
    });
    if (!currency) {
      throw new BadRequestException('Mata uang tidak ditemukan atau tidak aktif');
    }
  }

  async validateBank(bankId: bigint): Promise<void> {
    const bank = await this.prisma.erpBank.findFirst({
      where: { id: BigInt(bankId), deletedAt: null, isActive: true },
      select: { id: true },
    });
    if (!bank) {
      throw new BadRequestException('Bank tidak ditemukan atau tidak aktif');
    }
  }

  deriveLevel(parent: AccountParent | null): number {
    return parent ? parent.level + 1 : 1;
  }

  assertTypeMatchesParent(
    accountType: ErpAccountType | undefined,
    parentType: ErpAccountType,
  ): ErpAccountType {
    if (accountType && accountType !== parentType) {
      throw new BadRequestException(
        `Tipe akun (${accountType}) harus sama dengan tipe parent (${parentType})`,
      );
    }
    return parentType;
  }

  assertLeafDetails(
    hasMoneyOrBank: boolean,
    isLeaf: boolean,
  ): void {
    if (hasMoneyOrBank && !isLeaf) {
      throw new BadRequestException(
        'Mata uang, no. rekening, dan bank hanya boleh diisi untuk akun di segmen terakhir (leaf)',
      );
    }
  }

  assertPostableHasNoChildren(kind: 'HEADER' | 'POSTABLE', childCount: number): void {
    if (kind === 'POSTABLE' && childCount > 0) {
      throw new BadRequestException(
        'Akun dengan anak tidak bisa dijadikan POSTABLE; POSTABLE tidak boleh memiliki anak',
      );
    }
  }

  isLeaf(code: string, format: AccountCodeFormat): boolean {
    return isLeafAccountCode(code, format);
  }

  kindFromCode(code: string, format: AccountCodeFormat): 'HEADER' | 'POSTABLE' {
    return accountKindFromCode(code, format);
  }

  normalBalanceOf(type: ErpAccountType) {
    return normalBalanceForAccountType(type);
  }

  async recomputeSubtreeLevels(rootId: bigint, newRootLevel: number): Promise<void> {
    const rootIdBig = BigInt(rootId);
    await this.walkAndSetLevel(rootIdBig, newRootLevel);
  }

  private async walkAndSetLevel(accountId: bigint, level: number): Promise<void> {
    await this.prisma.erpAccount.update({
      where: { id: accountId },
      data: { level },
    });
    const children = await this.prisma.erpAccount.findMany({
      where: { parentId: accountId, deletedAt: null },
      select: { id: true },
    });
    await Promise.all(
      children.map((child) => this.walkAndSetLevel(child.id, level + 1)),
    );
  }
}