-- ============================================================================
-- Script: 03_create_operational_tables.sql
-- Purpose: Create the 3 operational / configuration tables
--          CostMatrixConfig, Predictions, ModelComparison
-- Note:    ALL business parameters are stored here — NEVER hardcoded
-- ============================================================================

USE [ReturnManagementDB];
GO

-- ──────────────────────────────────────────────
-- 7. CostMatrixConfig  (Dynamic Configuration)
--    Stores financial parameters that drive the
--    cost-sensitive prediction framework.
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.CostMatrixConfig', 'U') IS NOT NULL
    DROP TABLE dbo.CostMatrixConfig;
GO

CREATE TABLE dbo.CostMatrixConfig (
    ConfigId                INT            NOT NULL PRIMARY KEY IDENTITY(1,1),
    ProfitMarginPercentage  DECIMAL(5,4)   NOT NULL DEFAULT 0.2000,
    HandlingCostPerOrder    DECIMAL(10,2)  NOT NULL DEFAULT 5.00,
    DynamicThreshold        DECIMAL(5,4)   NOT NULL DEFAULT 0.6400,
    ActiveStatus            BIT            NOT NULL DEFAULT 1,
    UpdatedAt               DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

PRINT '  ✅ CostMatrixConfig created.';
GO

-- ──────────────────────────────────────────────
-- 8. Predictions  (Live Inference Persistence)
--    Every prediction made by the system is
--    logged here for audit trail and analytics.
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.Predictions', 'U') IS NOT NULL
    DROP TABLE dbo.Predictions;
GO

CREATE TABLE dbo.Predictions (
    PredictionId        INT             NOT NULL PRIMARY KEY IDENTITY(1,1),
    OrderId             VARCHAR(50)     NOT NULL,
    ReturnProbability   FLOAT           NOT NULL,
    RiskCategory        VARCHAR(10)     NOT NULL
                        CONSTRAINT CK_Predictions_RiskCategory
                        CHECK (RiskCategory IN ('Green', 'Yellow', 'Red')),
    FalseNegativeCost   DECIMAL(12,2)   NOT NULL,
    FalsePositiveCost   DECIMAL(12,2)   NOT NULL,
    OptimalThreshold    FLOAT           NOT NULL,
    RecommendedAction   NVARCHAR(500)   NOT NULL,
    CreatedAt           DATETIME        NOT NULL DEFAULT GETDATE()
);
GO

CREATE NONCLUSTERED INDEX IX_Predictions_OrderId
    ON dbo.Predictions (OrderId);
CREATE NONCLUSTERED INDEX IX_Predictions_RiskCategory
    ON dbo.Predictions (RiskCategory);
CREATE NONCLUSTERED INDEX IX_Predictions_CreatedAt
    ON dbo.Predictions (CreatedAt DESC);
GO

PRINT '  ✅ Predictions created.';
GO

-- ──────────────────────────────────────────────
-- 9. ModelComparison  (Benchmark Metrics)
--    Stores the empirical evaluation metrics
--    from the thesis for the dashboard display.
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.ModelComparison', 'U') IS NOT NULL
    DROP TABLE dbo.ModelComparison;
GO

CREATE TABLE dbo.ModelComparison (
    ModelId               INT             NOT NULL PRIMARY KEY IDENTITY(1,1),
    ModelName             VARCHAR(100)    NOT NULL,
    ClassificationAccuracy FLOAT          NOT NULL,
    PrecisionScore        FLOAT           NOT NULL,
    RecallScore           FLOAT           NOT NULL,
    F1Score               FLOAT           NOT NULL,
    AUCROC                FLOAT           NOT NULL,
    FalseNegativesCount   INT             NOT NULL,
    FalsePositivesCount   INT             NOT NULL,
    TotalFinancialLoss    DECIMAL(12,2)   NOT NULL,
    OperationalSavings    DECIMAL(12,2)   NULL DEFAULT 0.00,
    IsOptimal             BIT             NOT NULL DEFAULT 0
);
GO

PRINT '  ✅ ModelComparison created.';
GO

PRINT '';
PRINT '══════════════════════════════════════════════';
PRINT '  ALL 3 OPERATIONAL TABLES CREATED SUCCESSFULLY';
PRINT '══════════════════════════════════════════════';
GO
