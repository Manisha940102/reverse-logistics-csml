-- ============================================================================
-- Script: 04_seed_config_and_benchmarks.sql
-- Purpose: Populate CostMatrixConfig and ModelComparison with the
--          exact empirical metrics from the MSc thesis and code outputs.
-- Source files cross-referenced:
--   - baseline_results.csv (Step 7)
--   - cost_sensitive_results.csv (Step 8 / Step 9)
-- ============================================================================

USE [ReturnManagementDB];
GO

-- ──────────────────────────────────────────────
-- Seed CostMatrixConfig
-- Thesis parameters: 20% profit margin,
-- R$ 5.00 handling cost, 0.65 optimal threshold
-- ──────────────────────────────────────────────
TRUNCATE TABLE dbo.CostMatrixConfig;
GO

INSERT INTO dbo.CostMatrixConfig
    (ProfitMarginPercentage, HandlingCostPerOrder, DynamicThreshold, ActiveStatus, UpdatedAt)
VALUES
    (0.2000, 5.00, 0.6500, 1, GETDATE());
GO

PRINT '  ✅ CostMatrixConfig seeded (1 active row: Margin 20%, Handling R$ 5.00, Threshold 0.65).';
GO

-- ──────────────────────────────────────────────
-- Seed ModelComparison
-- Master 6-Model Benchmark Matrix evaluated in the thesis:
--   Model 1: Variant 1: Cost-Unaware RF (Default t=0.50)
--   Model 2: Variant 2: Cost-Aware RF (Default t=0.50)
--   Model 3: Variant 3: Cost-Aware RF + OOF Threshold (t=0.29)
--   Model 4: Variant 1: Cost-Unaware XGBoost (Default t=0.50)
--   Model 5: Variant 2: Cost-Aware XGBoost (Default t=0.50)
--   Model 6: Variant 3: Cost-Aware XGBoost + OOF Threshold (t=0.65) [WINNER 🏆]
-- ──────────────────────────────────────────────
TRUNCATE TABLE dbo.ModelComparison;
GO

INSERT INTO dbo.ModelComparison
    (ModelName, ClassificationAccuracy, PrecisionScore, RecallScore, F1Score, AUCROC,
     FalseNegativesCount, FalsePositivesCount, TotalFinancialLoss, OperationalSavings, IsOptimal)
VALUES
    ('Model 1: Cost-Unaware RF (Default t=0.50)',                0.8997, 0.8280, 0.4511, 0.5841, 0.8062, 1898, 324,  106756.55, 0.00,    0),
    ('Model 2: Cost-Aware RF (Default t=0.50)',                  0.8994, 0.8241, 0.4526, 0.5843, 0.8032, 1893, 334,  105835.21, 921.34,  0),
    ('Model 3: Cost-Aware RF + OOF Threshold (t=0.29)',          0.8946, 0.7413, 0.4988, 0.5964, 0.8032, 1733, 603,  103490.11, 3266.44, 0),
    ('Model 4: Cost-Unaware XGBoost (Default t=0.50)',           0.9006, 0.8363, 0.4520, 0.5868, 0.8047, 1895, 306,  106343.49, 413.06,  0),
    ('Model 5: Cost-Aware XGBoost (Default t=0.50)',             0.8533, 0.5279, 0.5697, 0.5480, 0.7983, 1488, 1762, 112402.29, -5645.74,0),
    ('Model 6: Cost-Aware XGBoost + OOF Threshold (t=0.65)',     0.8944, 0.7379, 0.5023, 0.5977, 0.7983, 1721, 616,  102753.95, 4002.60, 1);
GO

PRINT '  ✅ ModelComparison seeded (6 master benchmark models).';
GO

SELECT ModelName, ClassificationAccuracy, PrecisionScore, RecallScore, F1Score, TotalFinancialLoss, OperationalSavings, IsOptimal
FROM dbo.ModelComparison
ORDER BY TotalFinancialLoss ASC;
GO
