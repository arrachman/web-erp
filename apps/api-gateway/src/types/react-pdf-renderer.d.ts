declare module '@react-pdf/renderer' {
  import type { ComponentType, ReactElement } from 'react';

  export const Document: ComponentType<any>;
  export const Page: ComponentType<any>;
  export const View: ComponentType<any>;
  export const Text: ComponentType<any>;
  export const Image: ComponentType<any>;

  export function renderToBuffer(element: ReactElement): Promise<Buffer>;
}
