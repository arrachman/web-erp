'use client';

import * as React from 'react';
import { SearchSelectProps } from './search-select-types';
import { useSearchSelect } from './use-search-select';
import { SearchSelectTrigger } from './search-select-trigger';
import { SearchSelectModal } from './search-select-modal';

export type { SearchSelectOption, SearchSelectColumn, SearchSelectProps } from './search-select-types';

export function SearchSelect(props: SearchSelectProps) {
  const { id } = props;
  const s = useSearchSelect(props);

  return (
    <>
      <SearchSelectTrigger
        id={id}
        isMulti={s.isMulti}
        disabled={props.disabled ?? false}
        error={props.error ?? false}
        fill={props.fill ?? false}
        placeholder={props.placeholder ?? 'Pilih…'}
        triggerDisplay={s.triggerDisplay}
        inputText={s.inputText}
        dropdownOpen={s.dropdownOpen}
        dropdownOptions={s.dropdownOptions}
        dropdownLoading={s.dropdownLoading}
        currentValue={s.isMulti ? undefined : props.value}
        triggerRef={s.triggerRef}
        dropdownRef={s.dropdownRef}
        onTriggerFocus={s.handleTriggerFocus}
        onSingleFocus={s.handleSingleFocus}
        onSingleBlur={s.handleSingleBlur}
        onSingleChange={s.handleSingleChange}
        onSingleKeyDown={s.handleSingleKeyDown}
        onIconMouseDown={s.handleIconMouseDown}
        onSelectFromDropdown={s.selectFromDropdown}
      />
      <SearchSelectModal
        open={s.open}
        onOpenChange={() => {}}
        resolvedTitle={s.resolvedTitle}
        isMulti={s.isMulti}
        loading={s.loading}
        total={s.total}
        totalPages={s.totalPages}
        page={s.page}
        limit={props.limit ?? 10}
        query={s.query}
        columns={s.columns}
        displayOptions={s.displayOptions}
        colSpan={s.colSpan}
        confirmCount={s.confirmCount}
        focusedIdx={s.focusedIdx}
        tableActive={s.tableActive}
        localSingle={s.localSingle}
        localSelected={s.localSelected}
        allChecked={s.allChecked}
        someChecked={s.someChecked}
        searchRef={s.searchRef}
        scrollRef={s.scrollRef}
        onKeyDown={s.handleKeyDown}
        onQueryChange={(e) => s.setQuery(e.target.value)}
        onToggleAll={s.toggleAll}
        onToggleMulti={s.toggleMulti}
        onSelectSingle={s.selectSingle}
        onConfirmRow={s.confirmRow}
        onClose={s.closeModal}
        onConfirm={s.confirm}
      />
    </>
  );
}
