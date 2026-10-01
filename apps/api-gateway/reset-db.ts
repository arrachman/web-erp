import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

const resetSQL = readFileSync(join(__dirname, 'scripts/reset-db/reset.sql'), 'utf8');

async function main() {
  console.log('Resetting database with m0_* schema...');
  try {
    await prisma.$executeRawUnsafe(resetSQL);
    console.log('Database reset completed successfully');
  } catch (error) {
    console.error('Error resetting database:', error instanceof Error ? error.message : String(error));
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

main();
