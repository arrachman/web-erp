/**
 * ERP page routes — ErpPageCtx interface + full ERP_PAGES path→component map.
 * Extracted from shell-route-renderer to keep files under 400 lines.
 * Keys/aliases preserved verbatim (incl. duplicate targets like
 * /master/item-info and /master/item-informations).
 */

import * as React from 'react';
import { makeTranslator } from '@/lib/mock';
import { SettingsPage } from '@/components/pages/settings';
import { AppearancePage } from '@/components/pages/appearance';
import { ErpBankAccountsPage } from '@/components/pages/bank-accounts-page';
import { ErpApprovalRulesPage } from '@/components/pages/approval-rules-page';
import { ErpHomeLayoutPage } from '@/components/pages/home-layout-page';
import { ErpRoleDocPoliciesPage } from '@/components/pages/role-doc-policies-page';
import { AccountCodeFormatPage } from '@/components/pages/account-code-format-page';
import { NumberFormatPage } from '@/components/pages/number-format-page';
import { DateFormatPage } from '@/components/pages/date-format-page';
import { ErpImportPage } from '@/components/pages/import-page';
import { ErpUsersPage } from '@/components/pages/users-page';
import { ErpRolesPage } from '@/components/pages/roles-page';
import { ErpSettingsPage } from '@/components/pages/settings-page';
import { ErpPermissionsPage } from '@/components/pages/permissions-page';
import { ErpMenusPage } from '@/components/pages/menus-page';
import { GridCustomizationPage } from '@/components/pages/grid-customization-page';
import { FormBuilderPage } from '@/components/pages/form-builder-page';
import { ReportStudioPage } from '@/components/pages/report-studio-page';
import { ErpDocumentNumberingsPage } from '@/components/pages/document-numberings-page';
import { ErpFiscalPeriodsPage } from '@/components/pages/fiscal-periods-page';
import { ErpAuditLogsPage } from '@/components/pages/audit-logs-page';
import { ErpLanguagePage } from '@/components/pages/language-page';
import { ErpOnlineUsersPage } from '@/components/pages/online-users-page';
import { ErpFiscalPeriodsClosePage } from '@/components/pages/fiscal-periods-close-page';
import { ErpRecalcCogsPage } from '@/components/pages/tools-recalc-cogs-page';
import { ErpRepostJournalPage } from '@/components/pages/tools-repost-journal-page';
import { ErpDataValidityPage } from '@/components/pages/tools-data-validity-page';
import { SettingsGroupPage } from '@/components/pages/settings-group-page';
import { ErpBranchesPage } from '@/components/pages/branches-page';
import { ErpItemsPage } from '@/components/pages/items-page';
import { ErpUnitsPage } from '@/components/pages/units-page';
import { ErpPartnersPage } from '@/components/pages/partners-page';
import { ErpVendorsPage } from '@/components/pages/vendors-page';
import { ErpItemCategoriesPage } from '@/components/pages/item-categories-page';
import { ErpLocationsPage } from '@/components/pages/locations-page';
import { ErpWarehousesPage } from '@/components/pages/warehouses-page';
import { ErpPartnerCategoriesPage } from '@/components/pages/partner-categories-page';
import { ErpPartnerTypesPage } from '@/components/pages/partner-types-page';
import { ErpAccountsPage } from '@/components/pages/accounts-page';
import { ErpCurrenciesPage } from '@/components/pages/currencies-page';
import { ErpTaxesPage } from '@/components/pages/taxes-page';
import { ErpPaymentTermsPage } from '@/components/pages/payment-terms-page';
import { ErpDivisionsPage } from '@/components/pages/divisions-page';
import { ErpSubDivisionsPage } from '@/components/pages/sub-divisions-page';
import { ErpProjectsPage } from '@/components/pages/projects-page';
import { ErpCostCentersPage } from '@/components/pages/cost-centers-page';
import { ErpDepartmentsPage } from '@/components/pages/departments-page';
import { ErpSubDepartmentsPage } from '@/components/pages/sub-departments-page';
import { ErpColorsPage } from '@/components/pages/colors-page';
import { ErpNozzlesPage } from '@/components/pages/nozzles-page';
import { ErpOemsPage } from '@/components/pages/oems-page';
import { ErpBrandsPage } from '@/components/pages/brands-page';
import { ErpMaterialsPage } from '@/components/pages/materials-page';
import { ErpItemModelsPage } from '@/components/pages/item-models-page';
import { ErpSizesPage } from '@/components/pages/sizes-page';
import { ErpSectionsPage } from '@/components/pages/sections-page';
import { ErpItemKindsPage } from '@/components/pages/item-types-page';
import { ErpProductClassesPage } from '@/components/pages/product-classes-page';
import { ErpClassesPage } from '@/components/pages/classes-page';
import { ErpBanksPage } from '@/components/pages/banks-page';
import { ErpExpeditionsPage } from '@/components/pages/expeditions-page';
import { ErpOtherCostsPage } from '@/components/pages/other-costs-page';
import { ErpCommissionsPage } from '@/components/pages/commissions-page';
import { ErpStockAdjustmentTypesPage } from '@/components/pages/stock-adjustment-types-page';
import { ErpCountriesPage } from '@/components/pages/countries-page';
import { ErpProvincesPage } from '@/components/pages/provinces-page';
import { ErpCitiesPage } from '@/components/pages/cities-page';
import { ErpAreasPage } from '@/components/pages/areas-page';
import { ErpStorageBinsPage } from '@/components/pages/storage-bins-page';
import { ErpPartnerSubCategoriesPage } from '@/components/pages/partner-sub-categories-page';
import { ErpPriceCategoriesPage } from '@/components/pages/price-categories-page';
import { ErpTransactionNotesPage } from '@/components/pages/transaction-notes-page';
import { ErpTxnNoteDetailsPage } from '@/components/pages/txn-note-details-page';
import { ErpPriceIndicesPage } from '@/components/pages/price-indices-page';
import { ErpItemInformationsPage } from '@/components/pages/item-informations-page';
import { ErpProductionCategoriesPage } from '@/components/pages/production-categories-page';
import { ErpPointCategoriesPage } from '@/components/pages/point-categories-page';
import { ErpMiscellaneousPage } from '@/components/pages/miscellaneous-page';
import { ErpSubClassesPage } from '@/components/pages/sub-classes-page';
import { ErpWorkEstimatesPage } from '@/components/pages/work-estimates-page';
import { ErpLaborsPage } from '@/components/pages/labors-page';
import { ErpMachinesPage } from '@/components/pages/machines-page';
import { ErpDesignersPage } from '@/components/pages/designers-page';
import { ErpProductionActivitiesPage } from '@/components/pages/production-activities-page';
import { ErpProductionRoutesPage } from '@/components/pages/production-routes-page';
import { ErpArReceiptsPage } from '@/components/pages/fin-ar-receipts-page';
import { ErpApPaymentsPage } from '@/components/pages/fin-ap-payments-page';
import { ErpLedgerPage } from '@/components/pages/fin-ledger-page';
import { ErpTrialBalancePage } from '@/components/pages/fin-trial-balance-page';
import { ErpMovementBalancePage } from '@/components/pages/fin-movement-balance-page';
import { ErpEquityChangesPage } from '@/components/pages/fin-equity-changes-page';
import { ErpBalanceSheetPage } from '@/components/pages/fin-balance-sheet-page';
import { ErpIncomeStatementPage } from '@/components/pages/fin-income-statement-page';
import { ErpCashFlowPage } from '@/components/pages/fin-cash-flow-page';
import { ErpDailyCashBankPage } from '@/components/pages/fin-daily-cash-bank-page';
import { ErpArCardPage } from '@/components/pages/fin-ar-card-page';
import { ErpArAgingPage } from '@/components/pages/fin-ar-aging-page';
import { ErpApCardPage } from '@/components/pages/fin-ap-card-page';
import { ErpApAgingPage } from '@/components/pages/fin-ap-aging-page';
import { ErpGiroMaturityPage } from '@/components/pages/fin-giro-maturity-page';
import { ErpBudgetRealizationPage } from '@/components/pages/fin-budget-realization-page';
import { ErpCashbankTransfersPage } from '@/components/pages/fin-cashbank-transfers-page';
import {
  SlsRptQuotationsPage, SlsRptOrdersPage, SlsRptCustomerAdvancesPage,
  SlsRptPaymentReceiptsPage, SlsRptProformaInvoicesPage, SlsRptPackingListsPage,
  SlsRptDeliveryOrdersPage, SlsRptDeliveryReportsPage, SlsRptInvoicesPage,
  SlsRptFreightReceivablesPage, SlsRptReturnReceiptsPage, SlsRptReturnsPage,
  SlsRptArCollectionsPage, SlsRptArPaymentsPage, SlsRptInvoiceSwapsPage,
  SlsRptOpeningArBalancePage,
} from '@/components/pages/sls-report-doc-pages';
import {
  SlsSummaryPage, SlsByCustomerPage, SlsBySalesmanPage, SlsByItemPage,
  SlsByProjectPage, SlsByDivisionPage, SlsByCostCenterPage, SlsByItemCategoryPage,
  SlsRevenueCollectionPage, SlsByGroupPage,
} from '@/components/pages/sls-report-analytics-pages';
import { InvStatsKpiPage } from '@/components/pages/inv-stats-kpi-page';
import { InvStatsTopRevenuePage } from '@/components/pages/inv-stats-top-revenue-page';
import { InvStatsBestSellingPage } from '@/components/pages/inv-stats-best-selling-page';
import { InvStatsMostProfitablePage } from '@/components/pages/inv-stats-most-profitable-page';
import { InvStatsBelowMinimumPage } from '@/components/pages/inv-stats-below-minimum-page';
import { InvStatsApprovalsPage } from '@/components/pages/inv-stats-approvals-page';

export interface ErpPageCtx {
  t: ReturnType<typeof makeTranslator>;
}

export const ERP_PAGES: Record<string, (ctx: ErpPageCtx) => React.ReactNode> = {
  '/admin/settings/company': () => <SettingsGroupPage group="company" title="Pengaturan Perusahaan" />,
  '/admin/settings/accounting': () => <SettingsGroupPage group="accounting" title="Pengaturan Akuntansi" />,
  '/admin/settings/bank-accounts': () => <ErpBankAccountsPage />,
  '/admin/settings/tax': () => <SettingsGroupPage group="tax" title="Pengaturan Pajak" />,
  '/admin/settings/description': () => <SettingsGroupPage group="description" title="Deskripsi Dokumen" />,
  '/admin/settings/format': () => <SettingsGroupPage group="format" title="Format Tampilan" />,
  '/admin/settings/defaults': () => <SettingsGroupPage group="defaults" title="Nilai Default" />,
  '/admin/settings/report-defaults': () => <SettingsGroupPage group="report-defaults" title="Default Laporan" />,
  '/admin/settings/signature': () => <SettingsGroupPage group="signature" title="Tanda Tangan" />,
  '/admin/settings/options': () => <SettingsGroupPage group="options" title="Opsi Lanjutan" />,
  '/admin/settings/home': () => <ErpHomeLayoutPage />,
  '/admin/settings/approval': () => <ErpApprovalRulesPage />,
  '/admin/settings/doc-creation-policies': () => <ErpRoleDocPoliciesPage />,
  '/admin/account-code-format': () => <AccountCodeFormatPage />,
  '/admin/number-format': () => <NumberFormatPage />,
  '/admin/date-format': () => <DateFormatPage />,
  '/admin/import': () => <ErpImportPage />,
  '/admin/users': () => <ErpUsersPage />,
  '/admin/roles': () => <ErpRolesPage />,
  '/admin/settings': () => <ErpSettingsPage />,
  '/admin/permissions': () => <ErpPermissionsPage />,
  '/admin/menus': () => <ErpMenusPage />,
  '/admin/grid-customization': () => <GridCustomizationPage />,
  '/admin/form-builder': () => <FormBuilderPage />,
  '/admin/report-designer': () => <ReportStudioPage />,
  '/admin/document-numbering': () => <ErpDocumentNumberingsPage />,
  '/admin/fiscal-periods': () => <ErpFiscalPeriodsPage />,
  '/admin/audit-logs': () => <ErpAuditLogsPage />,
  '/admin/language': () => <ErpLanguagePage />,
  '/admin/users/online': () => <ErpOnlineUsersPage />,
  '/admin/fiscal-periods/close': () => <ErpFiscalPeriodsClosePage />,
  '/admin/tools/recalc-cogs': () => <ErpRecalcCogsPage />,
  '/admin/tools/repost-journal': () => <ErpRepostJournalPage />,
  '/admin/tools/data-validity': () => <ErpDataValidityPage />,
  '/admin/preferences': (ctx) => <SettingsPage t={ctx.t} />,
  '/master/branches': () => <ErpBranchesPage />,
  '/master/items': () => <ErpItemsPage />,
  '/master/units': () => <ErpUnitsPage />,
  '/master/partners': () => <ErpPartnersPage />,
  '/master/vendors': () => <ErpVendorsPage />,
  '/master/item-categories': () => <ErpItemCategoriesPage />,
  '/master/locations': () => <ErpLocationsPage />,
  '/master/warehouses': () => <ErpWarehousesPage />,
  '/master/partner-categories': () => <ErpPartnerCategoriesPage />,
  '/master/partner-types': () => <ErpPartnerTypesPage />,
  '/master/accounts': () => <ErpAccountsPage />,
  '/master/currencies': () => <ErpCurrenciesPage />,
  '/master/taxes': () => <ErpTaxesPage />,
  '/master/payment-terms': () => <ErpPaymentTermsPage />,
  '/master/divisions': () => <ErpDivisionsPage />,
  '/master/subdivisions': () => <ErpSubDivisionsPage />,
  '/master/colors': () => <ErpColorsPage />,
  '/master/nozzles': () => <ErpNozzlesPage />,
  '/master/oems': () => <ErpOemsPage />,
  '/master/brands': () => <ErpBrandsPage />,
  '/master/materials': () => <ErpMaterialsPage />,
  '/master/models': () => <ErpItemModelsPage />,
  '/master/sizes': () => <ErpSizesPage />,
  '/master/sections': () => <ErpSectionsPage />,
  '/master/item-types': () => <ErpItemKindsPage />,
  '/master/product-classes': () => <ErpProductClassesPage />,
  '/master/classes': () => <ErpClassesPage />,
  '/master/banks': () => <ErpBanksPage />,
  '/master/expeditions': () => <ErpExpeditionsPage />,
  '/master/other-costs': () => <ErpOtherCostsPage />,
  '/master/commissions': () => <ErpCommissionsPage />,
  '/master/stock-adjustment-types': () => <ErpStockAdjustmentTypesPage />,
  // legacy path alias (menu may still be cached until seed/migrate)
  '/master/item-txn-types': () => <ErpStockAdjustmentTypesPage />,
  '/master/countries': () => <ErpCountriesPage />,
  '/master/provinces': () => <ErpProvincesPage />,
  '/master/cities': () => <ErpCitiesPage />,
  '/master/areas': () => <ErpAreasPage />,
  '/master/storage-bins': () => <ErpStorageBinsPage />,
  '/master/customer-categories': () => <ErpPartnerSubCategoriesPage type="CUSTOMER" />,
  '/master/supplier-categories': () => <ErpPartnerSubCategoriesPage type="SUPPLIER" />,
  '/master/salesman-categories': () => <ErpPartnerSubCategoriesPage type="SALESMAN" />,
  '/master/price-categories': () => <ErpPriceCategoriesPage />,
  '/master/transaction-notes': () => <ErpTransactionNotesPage />,
  '/master/txn-note-details': () => <ErpTxnNoteDetailsPage />,
  '/master/price-indices': () => <ErpPriceIndicesPage />,
  '/master/item-info': () => <ErpItemInformationsPage />,
  '/master/item-informations': () => <ErpItemInformationsPage />,
  '/master/production-categories': () => <ErpProductionCategoriesPage />,
  '/master/point-categories': () => <ErpPointCategoriesPage />,
  '/master/miscellaneous': () => <ErpMiscellaneousPage />,
  '/master/sub-classes': () => <ErpSubClassesPage />,
  '/master/work-estimates': () => <ErpWorkEstimatesPage />,
  '/master/labors': () => <ErpLaborsPage />,
  '/master/machines': () => <ErpMachinesPage />,
  '/master/designers': () => <ErpDesignersPage />,
  '/master/production-activities': () => <ErpProductionActivitiesPage />,
  '/master/production-routes': () => <ErpProductionRoutesPage />,
  '/org/branches': () => <ErpBranchesPage />,
  '/org/locations': () => <ErpLocationsPage />,
  '/org/warehouses': () => <ErpWarehousesPage />,
  '/org/divisions': () => <ErpDivisionsPage />,
  '/org/sub-divisions': () => <ErpSubDivisionsPage />,
  '/org/projects': () => <ErpProjectsPage />,
  '/org/cost-centers': () => <ErpCostCentersPage />,
  '/org/departments': () => <ErpDepartmentsPage />,
  '/org/sub-departments': () => <ErpSubDepartmentsPage />,
  '/settings/appearance': (ctx) => <AppearancePage t={ctx.t} />,
  'set-appearance': (ctx) => <AppearancePage t={ctx.t} />,
  'set-prefs': (ctx) => <SettingsPage t={ctx.t} />,
  '/finance/receipt-memos': () => <ErpArReceiptsPage />,
  '/finance/send-memos': () => <ErpApPaymentsPage />,
  '/finance/ledger': () => <ErpLedgerPage />,
  '/finance/trial-balance': () => <ErpTrialBalancePage />,
  '/finance/movement-balance': () => <ErpMovementBalancePage />,
  '/finance/balance-sheet': () => <ErpBalanceSheetPage />,
  '/finance/income-statement': () => <ErpIncomeStatementPage />,
  '/finance/equity-changes': () => <ErpEquityChangesPage />,
  '/finance/cash-flow': () => <ErpCashFlowPage />,
  '/finance/daily-cash-bank': () => <ErpDailyCashBankPage />,
  '/finance/ar-card': () => <ErpArCardPage />,
  '/finance/ar-aging': () => <ErpArAgingPage />,
  '/finance/ap-card': () => <ErpApCardPage />,
  '/finance/ap-aging': () => <ErpApAgingPage />,
  '/finance/giro-maturity': () => <ErpGiroMaturityPage />,
  '/finance/budget-realization': () => <ErpBudgetRealizationPage />,
  '/finance/cashbank-transfers': () => <ErpCashbankTransfersPage />,
  // Sales (M5) reports
  '/sales/reports/quotations': () => <SlsRptQuotationsPage />,
  '/sales/reports/orders': () => <SlsRptOrdersPage />,
  '/sales/reports/customer-advances': () => <SlsRptCustomerAdvancesPage />,
  '/sales/reports/payment-receipts': () => <SlsRptPaymentReceiptsPage />,
  '/sales/reports/proforma-invoices': () => <SlsRptProformaInvoicesPage />,
  '/sales/reports/packing-lists': () => <SlsRptPackingListsPage />,
  '/sales/reports/delivery-orders': () => <SlsRptDeliveryOrdersPage />,
  '/sales/reports/delivery-reports': () => <SlsRptDeliveryReportsPage />,
  '/sales/reports/invoices': () => <SlsRptInvoicesPage />,
  '/sales/reports/freight-receivables': () => <SlsRptFreightReceivablesPage />,
  '/sales/reports/return-receipts': () => <SlsRptReturnReceiptsPage />,
  '/sales/reports/returns': () => <SlsRptReturnsPage />,
  '/sales/reports/ar-collections': () => <SlsRptArCollectionsPage />,
  '/sales/reports/ar-payments': () => <SlsRptArPaymentsPage />,
  '/sales/reports/invoice-swaps': () => <SlsRptInvoiceSwapsPage />,
  '/sales/reports/opening-ar-balance': () => <SlsRptOpeningArBalancePage />,
  '/sales/reports/summary': () => <SlsSummaryPage />,
  '/sales/reports/by-customer': () => <SlsByCustomerPage />,
  '/sales/reports/by-salesman': () => <SlsBySalesmanPage />,
  '/sales/reports/by-item': () => <SlsByItemPage />,
  '/sales/reports/by-project': () => <SlsByProjectPage />,
  '/sales/reports/by-division': () => <SlsByDivisionPage />,
  '/sales/reports/by-cost-center': () => <SlsByCostCenterPage />,
  '/sales/reports/by-item-category': () => <SlsByItemCategoryPage />,
  '/sales/reports/revenue-collection': () => <SlsRevenueCollectionPage />,
  '/sales/reports/by-group': () => <SlsByGroupPage />,
  // Warehouse (M3) statistics
  '/warehouse/stats/kpi': () => <InvStatsKpiPage />,
  '/warehouse/stats/top-revenue': () => <InvStatsTopRevenuePage />,
  '/warehouse/stats/best-selling': () => <InvStatsBestSellingPage />,
  '/warehouse/stats/most-profitable': () => <InvStatsMostProfitablePage />,
  '/warehouse/stats/below-minimum': () => <InvStatsBelowMinimumPage />,
  '/warehouse/stats/approvals': () => <InvStatsApprovalsPage />,
  // Legacy aliases
  'adm-users': () => <ErpUsersPage />,
  'adm-roles': () => <ErpRolesPage />,
  'adm-branches': () => <ErpBranchesPage />,
  'adm-settings': () => <ErpSettingsPage />,
  'md-items': () => <ErpItemsPage />,
  'md-units': () => <ErpUnitsPage />,
  'md-partners': () => <ErpPartnersPage />,
  'md-item-categories': () => <ErpItemCategoriesPage />,
};