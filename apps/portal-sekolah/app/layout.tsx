import type { Metadata } from 'next';
import './globals.css';

export const metadata: Metadata = {
  title: 'Portal Sekolah — CV Bahtera Madani',
  description:
    'Portal sekolah CV Bahtera Madani: katalog buku & cetak, pemesanan, dan tagihan untuk sekolah mitra.',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="id">
      <head>
        <link rel="preconnect" href="https://fonts.googleapis.com" />
        <link
          href="https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap"
          rel="stylesheet"
        />
      </head>
      <body>{children}</body>
    </html>
  );
}
