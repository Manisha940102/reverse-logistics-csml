-- ============================================================================
-- Script: 01_create_database.sql
-- Purpose: Drop (if exists) and recreate the ReturnManagementDB database
-- Target:  (LocalDb)\MSSQLLocalDB
-- Project: Cost-Sensitive ML Framework for Reverse Logistics
-- ============================================================================

USE [master];
GO

-- Close existing connections and drop the database
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'ReturnManagementDB')
BEGIN
    ALTER DATABASE [ReturnManagementDB] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [ReturnManagementDB];
END
GO

-- Create fresh database
CREATE DATABASE [ReturnManagementDB];
GO

USE [ReturnManagementDB];
GO

PRINT '✅ ReturnManagementDB created successfully.';
GO
