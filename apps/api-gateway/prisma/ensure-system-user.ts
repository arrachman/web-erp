import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

async function main() {
  const passwordHash =
    'pbkdf2$v1$sha512$210000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==';

  const systemUser = await prisma.user.upsert({
    where: { username: 'system' },
    update: {
      email: 'system@local.internal',
      fullName: 'System Background Process',
      isActive: false,
      deletedAt: null,
      deletedBy: null,
      updatedBy: null,
    },
    create: {
      email: 'system@local.internal',
      username: 'system',
      passwordHash,
      fullName: 'System Background Process',
      isActive: false,
      createdBy: null,
      updatedBy: null,
    },
    select: {
      id: true,
      email: true,
      username: true,
      isActive: true,
    },
  });

  const systemIdText = String(systemUser.id);
  await prisma.user.update({
    where: { id: systemUser.id },
    data: {
      createdBy: systemIdText,
      updatedBy: systemIdText,
      deletedBy: null,
    },
  });

  console.log('System user ready:', systemUser);
}

main()
  .catch((error) => {
    console.error(error);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
