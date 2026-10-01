"use client";

import * as React from "react";
import { Icon } from "@/components/ui/icons";
import { Kbd } from "@/components/ui/kbd";
import { REGISTRY, type RegistryRow } from "@/lib/registry";
import { fmtIDR, STATUSES, type Translator } from "@/lib/mock";
import {
  FilterChip,
  DateRangeChip,
  AddFilterChip,
} from "@/components/organisms/filter-chips";
import { useTabKey } from "@/lib/tab-context";
import { notify, bulkAction } from "@/lib/feedback";
import {
  type Setter,
  type CSS,
  type SortState,
  PAGE_SIZE,
  SORT_NUM_TYPES,
  DataTable,
} from "./data-list-parts";

interface DataListProps {
  moduleId: string;
  t: Translator;
  onNavigate: (r: string) => void;
  onOpenTab?: (r: string) => void;
}

/** Page header + filter toolbar — prototype `.page-header` / `.toolbar`. */
function ListHeader(p: {
  t: Translator;
  mod: { label: string; code: string };
  q: string;
  setQ: Setter<string>;
  status: string;
  setStatus: Setter<string>;
  dateOn: boolean;
  setDateOn: Setter<boolean>;
  range: { from: string; to: string };
  setRange: Setter<{ from: string; to: string }>;
  hasStatus: boolean;
  availFilters: { id: string; label?: string }[];
  filteredLen: number;
  sumTotal: number | null;
  openForm: () => void;
  setPage: Setter<number>;
  setSort: Setter<SortState>;
}) {
  const { t, mod, q, setQ, status, setStatus, dateOn, setDateOn } = p;
  const { range, setRange, hasStatus, availFilters, filteredLen } = p;
  const { sumTotal, openForm, setPage, setSort } = p;
  const muted: CSS = { fontSize: 'calc(11.5px * var(--font-scale, 1))' };
  return (
    <>
      <div className="page-header">
        <h1 className="page-title">
          {t(mod.label)}
          <span className="code-tag">{mod.code}</span>
        </h1>
        <div className="page-actions">
          <div className="search-input">
            <Icon name="search" size={12} />
            <input
              placeholder={t("Cari semua...")}
              value={q}
              onChange={(e) => {
                setQ(e.target.value);
                setPage(1);
              }}
            />
            <Kbd>/</Kbd>
          </div>
          <button
            className="btn"
            onClick={() =>
              notify(`${filteredLen} baris diekspor (.xlsx)`, "success")
            }
          >
            <Icon name="download" size={12} /> {t("Export")}
          </button>
          <button
            className="btn"
            onClick={() => notify("Data dimuat ulang", "info")}
          >
            <Icon name="refresh" size={12} />
          </button>
          <button className="btn primary" onClick={openForm}>
            <Icon name="plus" size={12} /> {t("Tambah")} <Kbd>N</Kbd>
          </button>
        </div>
      </div>
      <div className="toolbar">
        <Icon name="filter" size={13} className="muted" />
        {hasStatus && (
          <FilterChip
            label={t("Status")}
            val={status}
            options={["Semua", ...STATUSES]}
            onChange={(v) => {
              setStatus(v);
              setPage(1);
            }}
            onRemove={() => {
              setStatus("Semua");
              setPage(1);
            }}
          />
        )}
        {dateOn && (
          <DateRangeChip
            from={range.from}
            to={range.to}
            onChange={(f, to) => setRange({ from: f, to })}
            onRemove={() => setDateOn(false)}
          />
        )}
        <AddFilterChip
          available={availFilters.map((f) => ({ id: f.id, label: f.label }))}
          onAdd={() => setDateOn(true)}
          t={t}
        />
        <div style={{ flex: 1 }} />
        {sumTotal != null && (
          <span className="muted" style={muted}>
            Σ {fmtIDR(sumTotal)}
          </span>
        )}
        <span className="muted" style={muted}>
          · {filteredLen} {t("baris")}
        </span>
        <button
          className="btn ghost sm"
          onClick={() => {
            setStatus("Semua");
            setQ("");
            setSort({ col: null, dir: "asc" });
            setPage(1);
          }}
        >
          {t("Reset")}
        </button>
      </div>
    </>
  );
}

/** Pagination footer + selection bar — prototype `.pager` / `.bulk-bar`. */
function ListFooter(p: {
  t: Translator;
  safePage: number;
  totalPages: number;
  viewLen: number;
  filteredLen: number;
  selCount: number;
  setPage: Setter<number>;
  bulk: (kind: string) => void;
  clearSel: () => void;
}) {
  const { t, safePage, totalPages, viewLen, filteredLen, selCount } = p;
  const { setPage, bulk, clearSel } = p;
  return (
    <>
      <div className="pager">
        <span>
          {t("Halaman")}{" "}
          <strong style={{ color: "var(--fg)" }}>{safePage}</strong> {t("dari")}{" "}
          {totalPages}
        </span>
        <span>
          · {viewLen} {t("dari")} {filteredLen} {t("baris")}
        </span>
        <div className="spacer" />
        <span className="muted">
          Pintasan: <Kbd>J</Kbd>/<Kbd>K</Kbd> · <Kbd>X</Kbd> pilih ·{" "}
          <Kbd>N</Kbd> baru
        </span>
        <div className="seg">
          <button disabled={safePage === 1} onClick={() => setPage(1)}>
            <Icon name="chevdoubleleft" size={11} />
          </button>
          <button
            disabled={safePage === 1}
            onClick={() => setPage((x) => Math.max(1, x - 1))}
          >
            <Icon name="chevleft" size={11} />
          </button>
          <button
            disabled={safePage === totalPages}
            onClick={() => setPage((x) => Math.min(totalPages, x + 1))}
          >
            <Icon name="chevright" size={11} />
          </button>
          <button
            disabled={safePage === totalPages}
            onClick={() => setPage(totalPages)}
          >
            <Icon name="chevdoubleright" size={11} />
          </button>
        </div>
      </div>
      {selCount > 0 && (
        <div className="bulk-bar fade-in">
          <span className="count">{selCount}</span>
          <span>dipilih</span>
          <span className="divider" />
          <button className="ba-btn" onClick={() => bulk("approve")}>
            <Icon name="check" size={12} /> {t("Approve")}
          </button>
          <button className="ba-btn" onClick={() => bulk("post")}>
            <Icon name="play" size={12} /> {t("Posting")}
          </button>
          <button className="ba-btn" onClick={() => bulk("export")}>
            <Icon name="download" size={12} /> {t("Export")}
          </button>
          <span className="divider" />
          <button className="ba-btn danger" onClick={() => bulk("delete")}>
            <Icon name="trash" size={12} /> {t("Hapus")}
          </button>
          <span className="divider" />
          <button className="ba-btn" onClick={clearSel}>
            <Icon name="x" size={12} />
          </button>
        </div>
      )}
    </>
  );
}

/** Generic data list — ported from prototype `pages/data-list.jsx`. */
export function DataList({
  moduleId,
  t,
  onNavigate,
  onOpenTab,
}: DataListProps) {
  const openForm = () => (onOpenTab || onNavigate)(`${moduleId}-new`);
  const mod = REGISTRY[moduleId];

  const [rows] = React.useState<RegistryRow[]>(() => (mod ? mod.gen() : []));
  const [q, setQ] = React.useState("");
  const hasStatus = !!mod && mod.cols.some((c) => c.k === "status");
  const hasDate = !!mod && mod.cols.some((c) => c.t === "date");
  const [status, setStatus] = React.useState("Semua");
  const [dateOn, setDateOn] = React.useState(hasDate);
  const [range, setRange] = React.useState({
    from: "01/05/2026",
    to: "12/05/2026",
  });
  const [selected, setSelected] = React.useState<Set<number>>(new Set());
  const [focused, setFocused] = React.useState(0);
  const [sort, setSort] = React.useState<SortState>({ col: null, dir: "asc" });
  const [page, setPage] = React.useState(1);

  const cols = mod ? mod.cols : [];

  const filtered = React.useMemo(() => {
    let arr = rows;
    if (hasStatus && status !== "Semua")
      arr = arr.filter((r) => r.status === status);
    if (q) {
      const ql = q.toLowerCase();
      arr = arr.filter((r) =>
        cols.some((c) => String(r[c.k]).toLowerCase().includes(ql)),
      );
    }
    if (sort.col) {
      const scol = sort.col;
      const cdef = cols.find((c) => c.k === scol);
      const numeric = !!cdef && SORT_NUM_TYPES.includes(cdef.t);
      arr = [...arr].sort((a, b) => {
        const av = numeric ? Number(a[scol]) : a[scol];
        const bv = numeric ? Number(b[scol]) : b[scol];
        if (av < bv) return sort.dir === "asc" ? -1 : 1;
        if (av > bv) return sort.dir === "asc" ? 1 : -1;
        return 0;
      });
    }
    return arr;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rows, q, status, sort]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const safePage = Math.min(page, totalPages);
  const view = filtered.slice((safePage - 1) * PAGE_SIZE, safePage * PAGE_SIZE);

  const toggle = (id: number) =>
    setSelected((s) => {
      const ns = new Set(s);
      if (ns.has(id)) ns.delete(id);
      else ns.add(id);
      return ns;
    });
  const allSelected = view.length > 0 && view.every((r) => selected.has(r.id));
  const someSelected = view.some((r) => selected.has(r.id)) && !allSelected;
  const clearSel = () => setSelected(new Set());

  useTabKey((e) => {
    const tag = (e.target as HTMLElement).tagName;
    if (["INPUT", "TEXTAREA", "SELECT"].includes(tag)) return;
    if (e.key === "ArrowDown" || e.key === "j") {
      e.preventDefault();
      setFocused((f) => Math.min(view.length - 1, f + 1));
    } else if (e.key === "ArrowUp" || e.key === "k") {
      e.preventDefault();
      setFocused((f) => Math.max(0, f - 1));
    } else if (e.key === "x" || e.key === " ") {
      e.preventDefault();
      if (view[focused]) toggle(view[focused].id);
    } else if (e.key.toLowerCase() === "n") {
      e.preventDefault();
      openForm();
    }
  });

  if (!mod)
    return <div style={{ padding: 24 }}>Modul tidak ditemukan: {moduleId}</div>;

  const setSortCol = (col: string) =>
    setSort((s) => ({
      col,
      dir: s.col === col && s.dir === "asc" ? "desc" : "asc",
    }));
  const sortInd = (col: string) =>
    sort.col !== col ? null : (
      <span className="sort-ind">
        <Icon name={sort.dir === "asc" ? "chevup" : "chevdown"} size={10} />
      </span>
    );
  const totalCol = cols.find(
    (c) => c.t === "money" && /total|saldo|harga|anggaran|nilai/i.test(c.k),
  );
  const sumTotal = totalCol
    ? filtered.reduce((s, r) => s + Number(r[totalCol.k] || 0), 0)
    : null;
  const bulk = (kind: string) => bulkAction(kind, selected.size, clearSel);

  const availFilters = [
    hasStatus && { id: "status" },
    !dateOn && hasDate && { id: "tanggal", label: t("Tanggal") },
  ]
    .filter(Boolean)
    .filter((f) => (f as { id: string }).id !== "status") as {
    id: string;
    label?: string;
  }[];

  return (
    <div className="page">
      <ListHeader
        t={t}
        mod={mod}
        q={q}
        setQ={setQ}
        status={status}
        setStatus={setStatus}
        dateOn={dateOn}
        setDateOn={setDateOn}
        range={range}
        setRange={setRange}
        hasStatus={hasStatus}
        availFilters={availFilters}
        filteredLen={filtered.length}
        sumTotal={sumTotal}
        openForm={openForm}
        setPage={setPage}
        setSort={setSort}
      />
      <DataTable
        cols={cols}
        view={view}
        selected={selected}
        focused={focused}
        allSelected={allSelected}
        someSelected={someSelected}
        setSelected={setSelected}
        setFocused={setFocused}
        toggle={toggle}
        setSortCol={setSortCol}
        sortInd={sortInd}
      />
      <ListFooter
        t={t}
        safePage={safePage}
        totalPages={totalPages}
        viewLen={view.length}
        filteredLen={filtered.length}
        selCount={selected.size}
        setPage={setPage}
        bulk={bulk}
        clearSel={clearSel}
      />
    </div>
  );
}
