'use client';

import { SlsReportPage } from './sls-report-page';

export function SlsSummaryPage() {
  return <SlsReportPage reportKey="summary" title="Sales Summary" />;
}

export function SlsByCustomerPage() {
  return <SlsReportPage reportKey="by-customer" title="Sales by Customer" />;
}

export function SlsBySalesmanPage() {
  return <SlsReportPage reportKey="by-salesman" title="Sales by Salesman" />;
}

export function SlsByItemPage() {
  return <SlsReportPage reportKey="by-item" title="Sales by Item" />;
}

export function SlsByProjectPage() {
  return <SlsReportPage reportKey="by-project" title="Sales by Project" />;
}

export function SlsByDivisionPage() {
  return <SlsReportPage reportKey="by-division" title="Sales by Division" />;
}

export function SlsByCostCenterPage() {
  return <SlsReportPage reportKey="by-cost-center" title="Sales by Cost Center" />;
}

export function SlsByItemCategoryPage() {
  return <SlsReportPage reportKey="by-item-category" title="Sales by Item Category" />;
}

export function SlsRevenueCollectionPage() {
  return <SlsReportPage reportKey="revenue-collection" title="Revenue & Collection" />;
}

export function SlsByGroupPage() {
  return <SlsReportPage reportKey="by-group" title="Sales by Group" />;
}
