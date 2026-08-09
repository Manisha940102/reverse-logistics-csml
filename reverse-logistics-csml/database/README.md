# Database Setup — ReturnManagementDB

## Overview

This folder contains all SQL scripts and a Python data import tool to set up the
`ReturnManagementDB` database on SQL Server LocalDB.

## Prerequisites

- **SQL Server LocalDB** running as `(LocalDb)\MSSQLLocalDB`
- **ODBC Driver 17 for SQL Server** (comes with SQL Server)
- **Python 3.8+** with `pandas`, `pyodbc`, `sqlalchemy` (only for CSV import)

## Scripts (Run in order)

| # | Script | Purpose |
|---|--------|---------|
| 1 | `01_create_database.sql` | Drops existing DB and creates fresh `ReturnManagementDB` |
| 2 | `02_create_olist_tables.sql` | Creates 6 Olist raw data tables with PKs, FKs, indexes |
| 3 | `03_create_operational_tables.sql` | Creates `CostMatrixConfig`, `Predictions`, `ModelComparison` |
| 4 | `04_seed_config_and_benchmarks.sql` | Seeds config (15% margin, R$15 handling, 0.87 threshold) and 3 model benchmark rows |
| 5 | `05_import_olist_data.py` | Python script to bulk-import Olist CSVs into the tables |

## How to Run

### Option A: Run SQL scripts via SSMS
1. Open SQL Server Management Studio (SSMS)
2. Connect to `(LocalDb)\MSSQLLocalDB`
3. Execute scripts 01 → 04 in order
4. Then run the Python import:
   ```bash
   pip install pandas pyodbc sqlalchemy
   python 05_import_olist_data.py
   ```

### Option B: Run SQL scripts via sqlcmd
```powershell
sqlcmd -S "(LocalDb)\MSSQLLocalDB" -i 01_create_database.sql -E
sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d ReturnManagementDB -i 02_create_olist_tables.sql -E
sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d ReturnManagementDB -i 03_create_operational_tables.sql -E
sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d ReturnManagementDB -i 04_seed_config_and_benchmarks.sql -E
python 05_import_olist_data.py
```

## Connection String

For .NET backend (`appsettings.json`):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(LocalDb)\\MSSQLLocalDB;Database=ReturnManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

For Python (SQLAlchemy):
```python
"mssql+pyodbc://@(LocalDb)\\MSSQLLocalDB/ReturnManagementDB?driver=ODBC+Driver+17+for+SQL+Server&Trusted_Connection=yes"
```
