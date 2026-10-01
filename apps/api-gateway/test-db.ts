import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  console.log('Testing database connection...');
  try {
    const userCount = await prisma.user.count();
    console.log(`Total users: ${userCount}`);
    const users = await prisma.user.findMany({
      take: 5,
      select: { id: true, uuid: true, email: true, username: true },
    });
    console.log('Users:', users);
    console.log('Database connection successful');
  } catch (error) {
    console.error('Database connection error:', error);
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

main();
