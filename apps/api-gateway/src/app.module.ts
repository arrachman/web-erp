import { Module } from '@nestjs/common';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { APP_GUARD } from '@nestjs/core';
import { ScheduleModule } from '@nestjs/schedule';
import { ThrottlerGuard, ThrottlerModule } from '@nestjs/throttler';
import { PrismaModule } from './prisma/prisma.module';
import { RefCacheModule } from './common/cache/ref-cache.module';
import { HealthModule } from './health/health.module';
// ERP domain (web-erp) — auth + admin + org + items + partners + finance + system config
import { ErpAuthModule } from './erp-auth/erp-auth.module';
import { ErpAccountsModule } from './erp-accounts/erp-accounts.module';
import { ErpApprovalRulesModule } from './erp-approval-rules/erp-approval-rules.module';
import { ErpAreasModule } from './erp-areas/areas.module';
import { ErpAttachmentsModule } from './erp-attachments/erp-attachments.module';
import { ErpAuditModule } from './erp-audit/erp-audit.module';
import { ErpBankAccountsModule } from './erp-bank-accounts/erp-bank-accounts.module';
import { ErpBanksModule } from './erp-banks/banks.module';
import { ErpBranchesModule } from './erp-branches/erp-branches.module';
import { ErpBrandsModule } from './erp-brands/brands.module';
import { ErpCitiesModule } from './erp-cities/cities.module';
import { ErpClassesModule } from './erp-classes/classes.module';
import { ErpColorsModule } from './erp-colors/erp-colors.module';
import { ErpCommissionsModule } from './erp-commissions/commissions.module';
import { ErpCostCentersModule } from './erp-cost-centers/erp-cost-centers.module';
import { ErpCountriesModule } from './erp-countries/countries.module';
import { ErpCurrenciesModule } from './erp-currencies/erp-currencies.module';
import { ErpDepartmentsModule } from './erp-departments/erp-departments.module';
import { ErpDesignersModule } from './erp-designers/designers.module';
import { ErpDivisionsModule } from './erp-divisions/erp-divisions.module';
import { ErpDocumentNumberingsModule } from './erp-document-numberings/erp-document-numberings.module';
import { ErpExpeditionsModule } from './erp-expeditions/expeditions.module';
import { ErpFinApPaymentsModule } from './erp-fin-ap-payments/erp-fin-ap-payments.module';
import { ErpFinArReceiptsModule } from './erp-fin-ar-receipts/erp-fin-ar-receipts.module';
import { ErpFinCashBankTransactionsModule } from './erp-fin-cash-bank-transactions/erp-fin-cash-bank-transactions.module';
import { ErpFinGiroEntriesModule } from './erp-fin-giro-entries/erp-fin-giro-entries.module';
import { ErpFinGirosModule } from './erp-fin-giros/erp-fin-giros.module';
import { ErpFinJournalEntriesModule } from './erp-fin-journal-entries/erp-fin-journal-entries.module';
import { ErpFinLedgerModule } from './erp-fin-ledger/erp-fin-ledger.module';
import { ErpFinReportsModule } from './erp-fin-reports/erp-fin-reports.module';
import { ErpFinDocReportsModule } from './erp-fin-doc-reports/erp-fin-doc-reports.module';
import { ErpFinFxRevaluationsModule } from './erp-fin-fx-revaluations/erp-fin-fx-revaluations.module';
import { ErpFiscalPeriodsModule } from './erp-fiscal-periods/erp-fiscal-periods.module';
import { ErpFormFieldsModule } from './erp-form-fields/erp-form-fields.module';
import { ErpHomeWidgetsModule } from './erp-home-widgets/erp-home-widgets.module';
import { ErpImportModule } from './erp-import/erp-import.module';
import { ErpInvDailyChecksModule } from './erp-inv-daily-checks/erp-inv-daily-checks.module';
import { ErpInvGlModule } from './erp-inv-gl/erp-inv-gl.module';
import { ErpInvOpeningStocksModule } from './erp-inv-opening-stocks/erp-inv-opening-stocks.module';
import { ErpInvPriceAdjustmentsModule } from './erp-inv-price-adjustments/erp-inv-price-adjustments.module';
import { ErpInvReportsModule } from './erp-inv-reports/erp-inv-reports.module';
import { ErpInvStatsModule } from './erp-inv-stats/erp-inv-stats.module';
import { ErpInvStockAdjustmentsModule } from './erp-inv-stock-adjustments/erp-inv-stock-adjustments.module';
import { ErpInvStockCountsModule } from './erp-inv-stock-counts/erp-inv-stock-counts.module';
import { ErpInvStockMovementsModule } from './erp-inv-stock-movements/erp-inv-stock-movements.module';
import { ErpInvLotsModule } from './erp-inv-lots/erp-inv-lots.module';
import { ErpInvWeighbridgeTicketsModule } from './erp-inv-weighbridge-tickets/erp-inv-weighbridge-tickets.module';
import { ErpItemCategoriesModule } from './erp-item-categories/erp-item-categories.module';
import { ErpItemInformationsModule } from './erp-item-informations/erp-item-informations.module';
import { ErpItemModelsModule } from './erp-item-models/item-models.module';
import { ErpItemsModule } from './erp-items/erp-items.module';
import { ErpItemKindsModule } from './erp-item-types/item-types.module';
import { ErpLaborsModule } from './erp-labors/labors.module';
import { ErpLanguagesModule } from './erp-languages/erp-languages.module';
import { ErpLocationsModule } from './erp-locations/erp-locations.module';
import { ErpMachinesModule } from './erp-machines/machines.module';
import { ErpMaterialsModule } from './erp-materials/materials.module';
import { ErpMfgBomsModule } from './erp-mfg-boms/erp-mfg-boms.module';
import { ErpMfgWorkOrdersModule } from './erp-mfg-work-orders/erp-mfg-work-orders.module';
import { ErpMfgPrintEstimatesModule } from './erp-mfg-print-estimates/erp-mfg-print-estimates.module';
import { ErpMfgPrintJobsModule } from './erp-mfg-print-jobs/erp-mfg-print-jobs.module';
import { ErpMfgJobCostsModule } from './erp-mfg-job-costs/erp-mfg-job-costs.module';
import { ErpMfgSchedulesModule } from './erp-mfg-schedules/erp-mfg-schedules.module';
import { ErpMfgVdpModule } from './erp-mfg-vdp/erp-mfg-vdp.module';
import { ErpPortalModule } from './erp-portal/erp-portal.module';
import { ErpContractsModule } from './erp-contracts/erp-contracts.module';
import { ErpMiscellaneousModule } from './erp-miscellaneous/miscellaneous.module';
import { ErpNotificationsModule } from './erp-notifications/erp-notifications.module';
import { ErpNozzlesModule } from './erp-nozzles/nozzles.module';
import { ErpOemsModule } from './erp-oems/oems.module';
import { ErpOtherCostsModule } from './erp-other-costs/other-costs.module';
import { ErpPartnerCategoriesModule } from './erp-partner-categories/erp-partner-categories.module';
import { ErpPartnersModule } from './erp-partners/erp-partners.module';
import { ErpPartnerSubCategoriesModule } from './erp-partner-sub-categories/partner-sub-categories.module';
import { ErpPartnerTypesModule } from './erp-partner-types/erp-partner-types.module';
import { ErpPaymentTermsModule } from './erp-payment-terms/erp-payment-terms.module';
import { ErpPermissionsModule } from './erp-permissions/erp-permissions.module';
import { ErpPointCategoriesModule } from './erp-point-categories/point-categories.module';
import { ErpPriceCategoriesModule } from './erp-price-categories/price-categories.module';
import { ErpPriceIndicesModule } from './erp-price-indices/erp-price-indices.module';
import { ErpProductClassesModule } from './erp-product-classes/product-classes.module';
import { ErpProductionActivitiesModule } from './erp-production-activities/production-activities.module';
import { ErpProductionCategoriesModule } from './erp-production-categories/production-categories.module';
import { ErpProductionRoutesModule } from './erp-production-routes/production-routes.module';
import { ErpProjectsModule } from './erp-projects/erp-projects.module';
import { ErpProvincesModule } from './erp-provinces/provinces.module';
import { ErpPurBidSelectionsModule } from './erp-pur-bid-selections/erp-pur-bid-selections.module';
import { ErpPurFreightPayablesModule } from './erp-pur-freight-payables/erp-pur-freight-payables.module';
import { ErpSlsFreightReceivablesModule } from './erp-sls-freight-receivables/erp-sls-freight-receivables.module';
import { ErpPurGoodsReceiptsModule } from './erp-pur-goods-receipts/erp-pur-goods-receipts.module';
import { ErpPurInvoicesModule } from './erp-pur-invoices/erp-pur-invoices.module';
import { ErpPurOrdersModule } from './erp-pur-orders/erp-pur-orders.module';
import { ErpPurPaymentSchedulesModule } from './erp-pur-payment-schedules/erp-pur-payment-schedules.module';
import { ErpPurReportsModule } from './erp-pur-reports/erp-pur-reports.module';
import { ErpPurRequisitionsModule } from './erp-pur-requisitions/erp-pur-requisitions.module';
import { ErpPurReturnsModule } from './erp-pur-returns/erp-pur-returns.module';
import { ErpPurRfqsModule } from './erp-pur-rfqs/erp-pur-rfqs.module';
import { ErpPurVendorAdvancesModule } from './erp-pur-vendor-advances/erp-pur-vendor-advances.module';
import { ErpReportsModule } from './erp-reports/erp-reports.module';
import { ReportEngineModule } from './erp-report-engine/report-engine.module';
import { ErpRoleDocPoliciesModule } from './erp-role-doc-policies/erp-role-doc-policies.module';
import { ErpRolesModule } from './erp-roles/erp-roles.module';
import { ErpSectionsModule } from './erp-sections/sections.module';
import { ErpSchoolsModule } from './erp-schools/erp-schools.module';
import { ErpOrderHubModule } from './erp-order-hub/erp-order-hub.module';
import { ErpDocumentPackagesModule } from './erp-document-packages/erp-document-packages.module';
import { ErpTaxSubledgerModule } from './erp-tax-subledger/erp-tax-subledger.module';
import { ErpItemCatalogModule } from './erp-item-catalog/erp-item-catalog.module';
import { ErpPurRebatesModule } from './erp-pur-rebates/erp-pur-rebates.module';
import { ErpSettingsModule } from './erp-settings/erp-settings.module';
import { ErpSizesModule } from './erp-sizes/sizes.module';
import { ErpSlsArCollectionsModule } from './erp-sls-ar-collections/erp-sls-ar-collections.module';
import { ErpSlsCustomerAdvancesModule } from './erp-sls-customer-advances/erp-sls-customer-advances.module';
import { ErpSlsDeliveryOrdersModule } from './erp-sls-delivery-orders/erp-sls-delivery-orders.module';
import { ErpSlsDeliveryReportsModule } from './erp-sls-delivery-reports/erp-sls-delivery-reports.module';
import { ErpSlsInvoicesModule } from './erp-sls-invoices/erp-sls-invoices.module';
import { ErpSlsInvoiceSwapsModule } from './erp-sls-invoice-swaps/erp-sls-invoice-swaps.module';
import { ErpSlsOrdersModule } from './erp-sls-orders/erp-sls-orders.module';
import { ErpSlsPackingListsModule } from './erp-sls-packing-lists/erp-sls-packing-lists.module';
import { ErpSlsPackingUnitsModule } from './erp-sls-packing-units/erp-sls-packing-units.module';
import { ErpSlsDeliveryTripsModule } from './erp-sls-delivery-trips/erp-sls-delivery-trips.module';
import { ErpSlsProformaInvoicesModule } from './erp-sls-proforma-invoices/erp-sls-proforma-invoices.module';
import { ErpSlsQuotationsModule } from './erp-sls-quotations/erp-sls-quotations.module';
import { ErpSlsReportsModule } from './erp-sls-reports/erp-sls-reports.module';
import { ErpSlsReturnReceiptsModule } from './erp-sls-return-receipts/erp-sls-return-receipts.module';
import { ErpSlsReturnsModule } from './erp-sls-returns/erp-sls-returns.module';
import { ErpStockAdjustmentTypesModule } from './erp-stock-adjustment-types/stock-adjustment-types.module';
import { ErpStorageBinsModule } from './erp-storage-bins/storage-bins.module';
import { ErpSubAreasModule } from './erp-sub-areas/sub-areas.module';
import { ErpSubClassesModule } from './erp-sub-classes/sub-classes.module';
import { ErpSubDepartmentsModule } from './erp-sub-departments/erp-sub-departments.module';
import { ErpSubDivisionsModule } from './erp-sub-divisions/erp-sub-divisions.module';
import { ErpSysMenusModule } from './erp-sys-menus/erp-sys-menus.module';
import { ErpSysTransactionGridsModule } from './erp-sys-transaction-grids/erp-sys-transaction-grids.module';
import { ErpTaxesModule } from './erp-taxes/erp-taxes.module';
import { ErpToolsModule } from './erp-tools/erp-tools.module';
import { ErpTransactionNotesModule } from './erp-transaction-notes/transaction-notes.module';
import { ErpTxnNoteDetailsModule } from './erp-txn-note-details/txn-note-details.module';
import { ErpUnitsModule } from './erp-units/erp-units.module';
import { ErpUserPreferencesModule } from './erp-user-preferences/erp-user-preferences.module';
import { ErpUsersModule } from './erp-users/erp-users.module';
import { ErpWarehousesModule } from './erp-warehouses/erp-warehouses.module';
import { ErpWorkEstimatesModule } from './erp-work-estimates/work-estimates.module';

@Module({
  imports: [
    RefCacheModule,
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: '.env',
    }),
    // Rate limit: env-tunable (default 600 req / 60s). Per-route override via
    // @Throttle / @SkipThrottle decorator. Override prod via THROTTLE_LIMIT=N /
    // THROTTLE_TTL=ms di .env.
    ThrottlerModule.forRootAsync({
      inject: [ConfigService],
      useFactory: (cfg: ConfigService) => [
        {
          ttl: cfg.get<number>('THROTTLE_TTL', 60_000),
          limit: cfg.get<number>('THROTTLE_LIMIT', 600),
        },
      ],
    }),
    ScheduleModule.forRoot(),
    PrismaModule,
    HealthModule,
    // ERP domain (web-erp)
    ErpAuthModule,
    ErpAccountsModule,
    ErpApprovalRulesModule,
    ErpAreasModule,
    ErpAttachmentsModule,
    ErpAuditModule,
    ErpBankAccountsModule,
    ErpBanksModule,
    ErpBranchesModule,
    ErpBrandsModule,
    ErpCitiesModule,
    ErpClassesModule,
    ErpColorsModule,
    ErpCommissionsModule,
    ErpCostCentersModule,
    ErpCountriesModule,
    ErpCurrenciesModule,
    ErpDepartmentsModule,
    ErpDesignersModule,
    ErpDivisionsModule,
    ErpDocumentNumberingsModule,
    ErpExpeditionsModule,
    // ERP m2 Finance
    ErpFinApPaymentsModule,
    ErpFinArReceiptsModule,
    ErpFinCashBankTransactionsModule,
    ErpFinGiroEntriesModule,
    ErpFinGirosModule,
    ErpFinJournalEntriesModule,
    ErpFinLedgerModule,
    ErpFinReportsModule,
    ErpFinDocReportsModule,
    ErpFinFxRevaluationsModule,
    ErpFiscalPeriodsModule,
    ErpFormFieldsModule,
    ErpHomeWidgetsModule,
    ErpImportModule,
    ErpInvDailyChecksModule,
    ErpInvGlModule,
    ErpInvOpeningStocksModule,
    ErpInvPriceAdjustmentsModule,
    ErpInvReportsModule,
    ErpInvStatsModule,
    ErpInvStockAdjustmentsModule,
    ErpInvStockCountsModule,
    ErpInvStockMovementsModule,
    ErpInvLotsModule,
    ErpInvWeighbridgeTicketsModule,
    ErpItemCategoriesModule,
    ErpItemInformationsModule,
    ErpItemModelsModule,
    ErpItemsModule,
    ErpItemKindsModule,
    ErpLaborsModule,
    ErpLanguagesModule,
    ErpLocationsModule,
    ErpMachinesModule,
    ErpMaterialsModule,
    ErpMfgBomsModule,
    ErpMfgWorkOrdersModule,
    ErpMfgPrintEstimatesModule,
    ErpMfgPrintJobsModule,
    ErpMfgJobCostsModule,
    ErpMfgSchedulesModule,
    ErpMfgVdpModule,
    ErpPortalModule,
    ErpContractsModule,
    ErpMiscellaneousModule,
    ErpNotificationsModule,
    ErpNozzlesModule,
    ErpOemsModule,
    ErpOtherCostsModule,
    ErpPartnerCategoriesModule,
    ErpPartnersModule,
    ErpPartnerSubCategoriesModule,
    ErpPartnerTypesModule,
    ErpPaymentTermsModule,
    ErpPermissionsModule,
    ErpPointCategoriesModule,
    ErpPriceCategoriesModule,
    ErpPriceIndicesModule,
    ErpProductClassesModule,
    ErpProductionActivitiesModule,
    ErpProductionCategoriesModule,
    ErpProductionRoutesModule,
    ErpProjectsModule,
    ErpProvincesModule,
    ErpPurBidSelectionsModule,
    ErpPurFreightPayablesModule,
    ErpSlsFreightReceivablesModule,
    ErpPurGoodsReceiptsModule,
    ErpPurInvoicesModule,
    ErpPurOrdersModule,
    ErpPurPaymentSchedulesModule,
    ErpPurReportsModule,
    ErpPurRequisitionsModule,
    ErpPurReturnsModule,
    ErpPurRfqsModule,
    ErpPurVendorAdvancesModule,
    ErpReportsModule,
    ReportEngineModule,
    ErpRoleDocPoliciesModule,
    ErpRolesModule,
    ErpSectionsModule,
    ErpSchoolsModule,
    ErpOrderHubModule,
    ErpDocumentPackagesModule,
    ErpTaxSubledgerModule,
    ErpItemCatalogModule,
    ErpPurRebatesModule,
    ErpSettingsModule,
    ErpSizesModule,
    // ERP m5 Sales
    ErpSlsArCollectionsModule,
    ErpSlsCustomerAdvancesModule,
    ErpSlsDeliveryOrdersModule,
    ErpSlsDeliveryReportsModule,
    ErpSlsInvoicesModule,
    ErpSlsInvoiceSwapsModule,
    ErpSlsOrdersModule,
    ErpSlsPackingListsModule,
    ErpSlsPackingUnitsModule,
    ErpSlsDeliveryTripsModule,
    ErpSlsProformaInvoicesModule,
    ErpSlsQuotationsModule,
    ErpSlsReportsModule,
    ErpSlsReturnReceiptsModule,
    ErpSlsReturnsModule,
    ErpStockAdjustmentTypesModule,
    ErpStorageBinsModule,
    ErpSubAreasModule,
    ErpSubClassesModule,
    ErpSubDepartmentsModule,
    ErpSubDivisionsModule,
    ErpSysMenusModule,
    ErpSysTransactionGridsModule,
    ErpTaxesModule,
    ErpToolsModule,
    ErpTransactionNotesModule,
    ErpTxnNoteDetailsModule,
    ErpUnitsModule,
    ErpUserPreferencesModule,
    ErpUsersModule,
    ErpWarehousesModule,
    ErpWorkEstimatesModule,
  ],
  providers: [
    // Global rate limiter (apply to all routes)
    { provide: APP_GUARD, useClass: ThrottlerGuard },
  ],
})
export class AppModule {}
