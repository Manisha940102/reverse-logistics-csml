"""
Script: 05_import_olist_data.py
Purpose: Import the 6 Olist CSV files from the raw data folder into SQL Server.
Target:  (LocalDb)\\MSSQLLocalDB -> ReturnManagementDB

Usage:
    python 05_import_olist_data.py

Requirements:
    pip install pandas pyodbc sqlalchemy

Notes:
    - Uses Windows Authentication (Trusted Connection)
    - Imports data in dependency order (customers & products first, then orders, etc.)
    - Handles NaN/NULL values automatically via pandas
    - The geolocation dataset is large (~1M rows) - this may take a few minutes
"""

import pandas as pd
from sqlalchemy import create_engine, text
import os
import sys
import time

# ---- Configuration --------------------------------------------------------
SERVER = r"(LocalDb)\MSSQLLocalDB"
DATABASE = "ReturnManagementDB"
CONNECTION_STRING = (
    f"mssql+pyodbc://@{SERVER}/{DATABASE}"
    f"?driver=ODBC+Driver+17+for+SQL+Server"
    f"&Trusted_Connection=yes"
)

# Path to the raw Olist CSV files
# Go up 2 levels: database/ -> reverse-logistics-csml/ -> Msc Final/
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.dirname(os.path.dirname(SCRIPT_DIR))  # Msc Final/
RAW_DATA_DIR = os.path.join(PROJECT_ROOT, "New", "Step 1 -Row data")

# Fallback: try relative from current working directory
if not os.path.isdir(RAW_DATA_DIR):
    RAW_DATA_DIR = os.path.join("New", "Step 1 -Row data")

if not os.path.isdir(RAW_DATA_DIR):
    print("[ERROR] Cannot find raw data directory: %s" % RAW_DATA_DIR)
    print("   Please ensure the Olist CSV files are in 'New/Step 1 -Row data/'")
    sys.exit(1)

# ---- CSV-to-Table mapping (in dependency order) ---------------------------
IMPORT_ORDER = [
    {
        "csv": "olist_customers_dataset.csv",
        "table": "olist_customers",
        "description": "Customers",
    },
    {
        "csv": "olist_products_dataset.csv",
        "table": "olist_products",
        "description": "Products",
    },
    {
        "csv": "olist_geolocation_dataset.csv",
        "table": "olist_geolocation",
        "description": "Geolocation (large - may take 1-2 minutes)",
    },
    {
        "csv": "olist_orders_dataset.csv",
        "table": "olist_orders",
        "description": "Orders",
    },
    {
        "csv": "olist_order_items_dataset.csv",
        "table": "olist_order_items",
        "description": "Order Items",
    },
    {
        "csv": "olist_order_reviews_dataset.csv",
        "table": "olist_order_reviews",
        "description": "Order Reviews",
    },
]


def import_csv(engine, csv_file, table_name, description):
    """Read a CSV file and bulk-insert into the target SQL Server table."""
    filepath = os.path.join(RAW_DATA_DIR, csv_file)

    if not os.path.isfile(filepath):
        print("  [SKIP] %s not found at %s" % (csv_file, filepath))
        return 0

    print("  [READ] Reading %s..." % csv_file)
    # Brazilian Portuguese CSVs may contain Latin-1 characters (e.g. accented letters)
    # Try UTF-8 first, fall back to latin-1
    try:
        df = pd.read_csv(filepath, low_memory=False, encoding="utf-8")
    except UnicodeDecodeError:
        df = pd.read_csv(filepath, low_memory=False, encoding="latin-1")

    # For order_reviews: parse dates explicitly to avoid SQL cast errors.
    if table_name == "olist_order_reviews":
        schema_cols = [
            "review_id", "order_id", "review_score", "review_comment_title",
            "review_comment_title_english", "review_comment_message",
            "review_comment_message_english", "review_creation_date",
            "review_answer_timestamp",
        ]
        df = df[[c for c in schema_cols if c in df.columns]]
        df["review_creation_date"] = pd.to_datetime(df["review_creation_date"], errors="coerce")
        df["review_answer_timestamp"] = pd.to_datetime(df["review_answer_timestamp"], errors="coerce")
        df["review_score"] = df["review_score"].astype(int)

    # For products: merge the English translation for product category
    if table_name == "olist_products":
        trans_file = os.path.join(RAW_DATA_DIR, "product_category_name_translation.csv")
        if os.path.isfile(trans_file):
            df_trans = pd.read_csv(trans_file)
            df = df.merge(df_trans, on="product_category_name", how="left")

    row_count = len(df)
    print("     -> %s rows loaded into memory." % f"{row_count:,}")

    print("  [WRITE] Writing to [%s]..." % table_name)
    start = time.time()

    # Use 'append' mode - tables were already created by the SQL scripts
    # chunksize helps with large datasets like geolocation (~1M rows)
    df.to_sql(
        name=table_name,
        con=engine,
        if_exists="append",
        index=False,
        chunksize=2000,
    )

    elapsed = time.time() - start
    print("  [OK] %s: %s rows imported in %.1fs" % (description, f"{row_count:,}", elapsed))
    return row_count


def main():
    print("=" * 60)
    print("  Olist Data Import -> ReturnManagementDB")
    print("=" * 60)
    print("  Server:    %s" % SERVER)
    print("  Database:  %s" % DATABASE)
    print("  Data Dir:  %s" % RAW_DATA_DIR)
    print()

    # Create SQLAlchemy engine
    try:
        engine = create_engine(CONNECTION_STRING, fast_executemany=True)
        # Test connection
        with engine.connect() as conn:
            conn.execute(text("SELECT 1"))
        print("  [CONNECTED] SQL Server connection successful.\n")
    except Exception as e:
        print("  [ERROR] Connection failed: %s" % e)
        print("  Please check that SQL Server is running and the instance name is correct.")
        sys.exit(1)

    total_rows = 0
    total_start = time.time()

    for item in IMPORT_ORDER:
        print("\n--- Importing: %s ---" % item["description"])
        rows = import_csv(engine, item["csv"], item["table"], item["description"])
        total_rows += rows

    total_elapsed = time.time() - total_start

    print("\n" + "=" * 60)
    print("  [DONE] IMPORT COMPLETE")
    print("     Total rows:  %s" % f"{total_rows:,}")
    print("     Total time:  %.1fs" % total_elapsed)
    print("=" * 60)


if __name__ == "__main__":
    main()
