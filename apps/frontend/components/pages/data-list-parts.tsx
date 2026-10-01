"use client";

import * as React from "react";
import { StatusBadge } from "@/components/ui/badge";
import { type RegistryCol, type RegistryRow } from "@/lib/registry";
import { fmtIDR } from "@/lib/mock";
import { notify } from "@/lib/feedback";

export type Setter<T> = React.Dispatch<React.SetStateAction<T>>;
export type CSS = React.CSSProperties;

export const NUM_TYPES = ["num", "qty", "qtyS", "money", "moneyS"];
export const SORT_NUM_TYPES = ["num", "qty", "qtyS", "money", "moneyS", "pct"];
export const PAGE_SIZE = 24;
const NUM_RIGHT: CSS = { display: "block", textAlign: "right" };
const PCT_TRACK: CSS = {
  flex: 1,
  height: 5,
  background: "var(--panel-2)",
  borderRadius: 3,
  overflow: "hidden",
};

export interface SortState {
  col: string | null;
  dir: "asc" | "desc";
}

/** Single typed table cell renderer — ported from prototype `DLCell`. */
function DLCell({ col, value }: { col: RegistryCol; value: string | number }) {
  const t = col.t;
  const n = Number(value);
  if (t === "date") return <span className="mono muted">{value}</span>;
  if (t === "status") return <StatusBadge status={String(value)} />;
  if (t === "email") return <span className="muted">{value}</span>;
  if (t === "code") {
    const st: CSS = { color: "var(--primary-soft-fg)", cursor: "pointer" };
    return (
      <span className="mono" style={st}>
        {value}
      </span>
    );
  }
  if (t === "ver")
    return (
      <span className="code" style={{ marginLeft: 0 }}>
        {value}
      </span>
    );
  if (t === "num" || t === "qty")
    return (
      <span className="num" style={NUM_RIGHT}>
        {n.toLocaleString("id-ID")}
      </span>
    );
  if (t === "qtyS") {
    const color =
      n < 0 ? "var(--danger)" : n > 0 ? "var(--success)" : "var(--fg-muted)";
    return (
      <span className="num" style={{ ...NUM_RIGHT, color }}>
        {n > 0 ? "+" : ""}
        {n.toLocaleString("id-ID")}
      </span>
    );
  }
  if (t === "money")
    return (
      <span className="num" style={NUM_RIGHT}>
        {fmtIDR(n)}
      </span>
    );
  if (t === "moneyS")
    return (
      <span
        className="num"
        style={{ ...NUM_RIGHT, color: n < 0 ? "var(--danger)" : "inherit" }}
      >
        {fmtIDR(n)}
      </span>
    );
  if (t === "pct") {
    const wrap: CSS = {
      display: "flex",
      alignItems: "center",
      gap: 8,
      minWidth: 110,
    };
    const bar: CSS = {
      width: `${value}%`,
      height: "100%",
      background: n >= 100 ? "var(--success)" : "var(--primary)",
    };
    const lbl: CSS = { fontSize: 'calc(11px * var(--font-scale, 1))', minWidth: 30, textAlign: "right" };
    return (
      <div style={wrap}>
        <div style={PCT_TRACK}>
          <div style={bar} />
        </div>
        <span className="mono muted" style={lbl}>
          {value}%
        </span>
      </div>
    );
  }
  const wide: CSS | undefined =
    col.w && col.w > 180
      ? {
          display: "block",
          maxWidth: col.w,
          overflow: "hidden",
          textOverflow: "ellipsis",
        }
      : undefined;
  return <span style={wide}>{value}</span>;
}

/** Data grid — ported from prototype `.tbl` block. */
export function DataTable(p: {
  cols: RegistryCol[];
  view: RegistryRow[];
  selected: Set<number>;
  focused: number;
  allSelected: boolean;
  someSelected: boolean;
  setSelected: Setter<Set<number>>;
  setFocused: Setter<number>;
  toggle: (id: number) => void;
  setSortCol: (col: string) => void;
  sortInd: (col: string) => React.ReactNode;
}) {
  const { cols, view, selected, focused, allSelected, someSelected } = p;
  const { setSelected, setFocused, toggle, setSortCol, sortInd } = p;
  return (
    <div className="tbl-wrap scrollbar">
      <table className="tbl">
        <thead>
          <tr>
            <th className="col-check">
              <input
                type="checkbox"
                className="checkbox"
                checked={allSelected}
                ref={(el) => {
                  if (el) el.indeterminate = someSelected;
                }}
                onChange={() =>
                  setSelected(
                    allSelected ? new Set() : new Set(view.map((r) => r.id)),
                  )
                }
              />
            </th>
            {cols.map((c) => (
              <th
                key={c.k}
                className={`sortable ${NUM_TYPES.includes(c.t) ? "col-num" : ""}`}
                onClick={() => setSortCol(c.k)}
                style={c.w ? { minWidth: c.w } : undefined}
              >
                {c.h}
                {sortInd(c.k)}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {view.map((r, i) => (
            <tr
              key={r.id}
              className={`${selected.has(r.id) ? "selected" : ""} ${i === focused ? "focused" : ""}`}
              onClick={() => setFocused(i)}
            >
              <td className="col-check">
                <input
                  type="checkbox"
                  className="checkbox"
                  checked={selected.has(r.id)}
                  onChange={() => toggle(r.id)}
                />
              </td>
              {cols.map((c) => (
                <td
                  key={c.k}
                  className={NUM_TYPES.includes(c.t) ? "col-num" : ""}
                  onClick={
                    c.t === "code"
                      ? () => notify(`${r[c.k]} — detail (prototype)`, "info")
                      : undefined
                  }
                >
                  <DLCell col={c} value={r[c.k]} />
                </td>
              ))}
            </tr>
          ))}
          {view.length === 0 && (
            <tr>
              <td colSpan={cols.length + 1} className="tbl-empty">
                Tidak ada data yang cocok dengan filter
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
