-- ============================================================================
-- Script: 05_update_config_to_final_data.sql
-- Purpose: Update active CostMatrixConfig row in SQL Server to match the
--          Final Data thesis pipeline parameters (0.65 threshold, 20% margin, R$ 5.00 handling).
-- ============================================================================

USE [ReturnManagementDB];
GO

UPDATE dbo.CostMatrixConfig
SET 
    ProfitMarginPercentage = 0.2000,
    HandlingCostPerOrder   = 5.00,
    DynamicThreshold       = 0.6500,
    UpdatedAt              = GETDATE()
WHERE ActiveStatus = 1;
GO

PRINT '  ✅ CostMatrixConfig updated successfully:';
PRINT '     - ProfitMarginPercentage = 0.20 (20%)';
PRINT '     - HandlingCostPerOrder   = R$ 5.00';
PRINT '     - DynamicThreshold       = 0.65 (65%)';
GO

SELECT ConfigId, ProfitMarginPercentage, HandlingCostPerOrder, DynamicThreshold, ActiveStatus, UpdatedAt
FROM dbo.CostMatrixConfig
WHERE ActiveStatus = 1;
GO
