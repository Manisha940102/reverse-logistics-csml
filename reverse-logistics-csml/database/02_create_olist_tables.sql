-- ============================================================================
-- Script: 02_create_olist_tables.sql
-- Purpose: Create the 6 raw Olist e-commerce dataset tables
-- Source:  Brazilian E-Commerce Public Dataset by Olist (Kaggle)
-- Note:    Field names preserve original Olist naming conventions
-- ============================================================================

USE [ReturnManagementDB];
GO

-- ──────────────────────────────────────────────
-- 1. Customers
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.olist_customers', 'U') IS NOT NULL
    DROP TABLE dbo.olist_customers;
GO

CREATE TABLE dbo.olist_customers (
    customer_id              VARCHAR(50)   NOT NULL PRIMARY KEY,
    customer_unique_id       VARCHAR(50)   NOT NULL,
    customer_zip_code_prefix INT           NOT NULL,
    customer_city            VARCHAR(100)  NULL,
    customer_state           VARCHAR(5)    NULL
);
GO

CREATE NONCLUSTERED INDEX IX_customers_unique_id
    ON dbo.olist_customers (customer_unique_id);
CREATE NONCLUSTERED INDEX IX_customers_zip
    ON dbo.olist_customers (customer_zip_code_prefix);
GO

PRINT '  ✅ olist_customers created.';
GO

-- ──────────────────────────────────────────────
-- 2. Geolocation
--    NOTE: No single unique PK – zip codes can
--    have multiple lat/lng centroids.
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.olist_geolocation', 'U') IS NOT NULL
    DROP TABLE dbo.olist_geolocation;
GO

CREATE TABLE dbo.olist_geolocation (
    geolocation_zip_code_prefix INT           NOT NULL,
    geolocation_lat             FLOAT         NOT NULL,
    geolocation_lng             FLOAT         NOT NULL,
    geolocation_city            VARCHAR(100)  NULL,
    geolocation_state           VARCHAR(5)    NULL
);
GO

CREATE NONCLUSTERED INDEX IX_geolocation_zip
    ON dbo.olist_geolocation (geolocation_zip_code_prefix);
GO

PRINT '  ✅ olist_geolocation created.';
GO

-- ──────────────────────────────────────────────
-- 3. Orders
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.olist_orders', 'U') IS NOT NULL
    DROP TABLE dbo.olist_orders;
GO

CREATE TABLE dbo.olist_orders (
    order_id                     VARCHAR(50)  NOT NULL PRIMARY KEY,
    customer_id                  VARCHAR(50)  NOT NULL,
    order_status                 VARCHAR(20)  NULL,
    order_purchase_timestamp     DATETIME     NULL,
    order_approved_at            DATETIME     NULL,
    order_delivered_carrier_date DATETIME     NULL,
    order_delivered_customer_date DATETIME    NULL,
    order_estimated_delivery_date DATETIME    NULL,

    CONSTRAINT FK_orders_customer
        FOREIGN KEY (customer_id)
        REFERENCES dbo.olist_customers (customer_id)
);
GO

CREATE NONCLUSTERED INDEX IX_orders_customer
    ON dbo.olist_orders (customer_id);
CREATE NONCLUSTERED INDEX IX_orders_status
    ON dbo.olist_orders (order_status);
GO

PRINT '  ✅ olist_orders created.';
GO

-- ──────────────────────────────────────────────
-- 4. Products
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.olist_products', 'U') IS NOT NULL
    DROP TABLE dbo.olist_products;
GO

CREATE TABLE dbo.olist_products (
    product_id                 VARCHAR(50)    NOT NULL PRIMARY KEY,
    product_category_name      NVARCHAR(100)  NULL,
    product_category_name_english NVARCHAR(100) NULL,
    product_name_lenght        INT            NULL,   -- Original Olist typo preserved
    product_description_lenght INT            NULL,   -- Original Olist typo preserved
    product_photos_qty         INT            NULL,
    product_weight_g           FLOAT          NULL,
    product_length_cm          FLOAT          NULL,
    product_height_cm          FLOAT          NULL,
    product_width_cm           FLOAT          NULL
);
GO

PRINT '  ✅ olist_products created.';
GO

-- ──────────────────────────────────────────────
-- 5. Order Items
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.olist_order_items', 'U') IS NOT NULL
    DROP TABLE dbo.olist_order_items;
GO

CREATE TABLE dbo.olist_order_items (
    order_id            VARCHAR(50)    NOT NULL,
    order_item_id       INT            NOT NULL,
    product_id          VARCHAR(50)    NOT NULL,
    seller_id           VARCHAR(50)    NOT NULL,
    shipping_limit_date DATETIME       NULL,
    price               DECIMAL(10,2)  NOT NULL,
    freight_value       DECIMAL(10,2)  NOT NULL,

    CONSTRAINT PK_order_items
        PRIMARY KEY (order_id, order_item_id),

    CONSTRAINT FK_order_items_order
        FOREIGN KEY (order_id)
        REFERENCES dbo.olist_orders (order_id),

    CONSTRAINT FK_order_items_product
        FOREIGN KEY (product_id)
        REFERENCES dbo.olist_products (product_id)
);
GO

CREATE NONCLUSTERED INDEX IX_order_items_product
    ON dbo.olist_order_items (product_id);
CREATE NONCLUSTERED INDEX IX_order_items_seller
    ON dbo.olist_order_items (seller_id);
GO

PRINT '  ✅ olist_order_items created.';
GO

-- ──────────────────────────────────────────────
-- 6. Order Reviews
-- ──────────────────────────────────────────────
IF OBJECT_ID('dbo.olist_order_reviews', 'U') IS NOT NULL
    DROP TABLE dbo.olist_order_reviews;
GO

CREATE TABLE dbo.olist_order_reviews (
    review_id                VARCHAR(50)    NOT NULL,
    order_id                 VARCHAR(50)    NOT NULL,
    review_score             INT            NOT NULL,
    review_comment_title     NVARCHAR(200)  NULL,
    review_comment_title_english   NVARCHAR(200)  NULL,
    review_comment_message   NVARCHAR(MAX)  NULL,
    review_comment_message_english NVARCHAR(MAX)  NULL,
    review_creation_date     DATETIME       NULL,
    review_answer_timestamp  DATETIME       NULL,

    CONSTRAINT PK_order_reviews
        PRIMARY KEY (review_id, order_id),

    CONSTRAINT FK_order_reviews_order
        FOREIGN KEY (order_id)
        REFERENCES dbo.olist_orders (order_id)
);
GO

CREATE NONCLUSTERED INDEX IX_order_reviews_order
    ON dbo.olist_order_reviews (order_id);
GO

PRINT '  ✅ olist_order_reviews created.';
GO

PRINT '';
PRINT '══════════════════════════════════════════';
PRINT '  ALL 6 OLIST TABLES CREATED SUCCESSFULLY';
PRINT '══════════════════════════════════════════';
GO
