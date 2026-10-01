import { afterEach, describe, expect, it } from 'vitest';
import { focusNextFrom, listFocusable } from '@/components/molecules/search-select/search-select-focus';

describe('focusNextFrom (SearchSelect post-submit)', () => {
  afterEach(() => {
    document.body.innerHTML = '';
  });

  it('moves focus from input A to input B', () => {
    document.body.innerHTML = `
      <input id="a" />
      <input id="b" />
      <input id="c" />
    `;
    const a = document.getElementById('a') as HTMLInputElement;
    const b = document.getElementById('b') as HTMLInputElement;
    a.focus();
    focusNextFrom(a);
    expect(document.activeElement).toBe(b);
  });

  it('selects text on the next text input', () => {
    document.body.innerHTML = `
      <input id="a" value="from" />
      <input id="b" value="next-value" />
    `;
    const a = document.getElementById('a') as HTMLInputElement;
    const b = document.getElementById('b') as HTMLInputElement;
    focusNextFrom(a);
    expect(document.activeElement).toBe(b);
    expect(b.selectionStart).toBe(0);
    expect(b.selectionEnd).toBe('next-value'.length);
  });

  it('skips disabled and hidden inputs', () => {
    document.body.innerHTML = `
      <input id="a" />
      <input id="disabled" disabled />
      <input id="hidden" type="hidden" />
      <input id="aria-hidden" aria-hidden="true" />
      <input id="b" />
    `;
    const a = document.getElementById('a') as HTMLInputElement;
    const b = document.getElementById('b') as HTMLInputElement;
    focusNextFrom(a);
    expect(document.activeElement).toBe(b);
  });

  it('skips elements inside aria-hidden ancestors (closed dialog shell)', () => {
    document.body.innerHTML = `
      <input id="a" />
      <div aria-hidden="true">
        <input id="dialog-ghost" />
      </div>
      <input id="b" />
    `;
    const a = document.getElementById('a') as HTMLInputElement;
    const b = document.getElementById('b') as HTMLInputElement;
    focusNextFrom(a);
    expect(document.activeElement).toBe(b);
  });

  it('no-ops when trigger is missing or already last', () => {
    document.body.innerHTML = `<input id="only" />`;
    const only = document.getElementById('only') as HTMLInputElement;
    only.focus();
    focusNextFrom(null);
    expect(document.activeElement).toBe(only);
    focusNextFrom(only);
    expect(document.activeElement).toBe(only);
  });

  it('listFocusable returns tab-order candidates', () => {
    document.body.innerHTML = `
      <input id="a" />
      <button type="button" id="btn">Go</button>
      <select id="s"><option>1</option></select>
      <input id="disabled" disabled />
    `;
    const ids = listFocusable().map((el) => el.id);
    expect(ids).toEqual(['a', 'btn', 's']);
  });
});
