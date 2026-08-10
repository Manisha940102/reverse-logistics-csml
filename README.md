# A Cost-Sensitive Machine Learning Framework for Optimizing Reverse Logistics Decisions in E-Commerce Return Management

**Author:** Manisha Oshadhi Jayasinghe (Student ID: 2585183)  
**Module:** 7CS041 – MSc Project Data Science  
**Supervisors:** Dr. Kasun Karunanayaka & Dr. W.M.K.S Ilmini  

---

##  Project Overview

This repository contains the full end-to-end implementation of a **Cost-Sensitive Machine Learning Framework** designed to optimize product return disposition decisions in e-commerce reverse logistics. 

Traditional machine learning classifiers focus on maximizing prediction accuracy, ignoring the asymmetric financial impact of classification errors. In reverse logistics:
- **False Negative (FN) Error (Failing to predict a return):** Incurs shipping fees, return freight charges, and warehouse handling fees ($C_{FN} = \text{Price} + \text{Roundtrip Freight} + \text{Handling}$).
- **False Positive (FP) Error (Rejecting a genuine customer order):** Incurs lost product profit margin ($C_{FP} = \text{Price} \times \text{Profit Margin}$).

This project resolves this financial asymmetry by:
1. Building a **Dynamic Cost Matrix** incorporating Haversine shipping distance and volumetric package weight.
2. Training a **Cost-Sensitive XGBoost Model** (`scale_pos_weight = 5.08`) with Out-Of-Fold (OOF) decision threshold optimization ($t^* = 0.61$).
3. Stratifying orders into three **Operational Risk Tiers**:
   - 🟢 **Green Risk Tier ($p < 0.21$):** Standard Automated Dispatch (Low Risk).
   - 🟡 **Yellow Risk Tier ($0.21 \le p < 0.61$):** Targeted Pre-Dispatch Intervention (Moderate Risk).
   - 🔴 **Red Risk Tier ($p \ge 0.61$):** High-Priority Return Mitigation / Returnless Refund Strategy (High Risk).
4. Deploying a full **3-Tier Enterprise Web Application** (Angular 17 UI, .NET 8 Web API, Python Flask ML Service, SQL Server DB).

---

##  Repository Structure

```
Msc Last/
│
├── data pipeline/                              # Complete Data Science & ML Pipeline (Steps 1 to 9.5)
│   ├── Step 1 - Data Integration & Cleaning/   # Raw Olist dataset ingestion & cleaning
│   ├── Step 2 - Data Preprocessing & Merging/  # Multi-table SQL/Pandas relational joins
│   ├── Step 3 - Feature Engineering/           # Volumetric Weight, Haversine Distance, Reviewer Deviance
│   ├── Step 4 - Data Leakage Prevention/       # Rolling cumulative sums replacing static aggregations
│   ├── Step 5 - Exploratory Data Analysis/     # EDA return distribution & correlation analysis
│   ├── Step 6 - Cost Matrix Construction/      # Dynamic Cost_FN and Cost_FP matrix calculation
│   ├── Step 7 - Baseline Model Training/       # Random Forest baseline variants (Models 1-3)
│   ├── Step 8 - Cost-Sensitive Tuning/         # XGBoost hyperparameter & threshold search (Models 4-6)
│   └── Step 9 - Final Evaluation & ROI/        # Tri-tier evaluation, 5-fold CV, ROI figures & serialization
│
├── reverse-logistics-csml/                      # Production 3-Tier Enterprise Web Application
│   ├── frontend-service/                       # Angular 17 Single Page Application (Port 4200)
│   ├── backend-service/                        # .NET 8 Web API & SQL Server Integration (Port 5265)
│   └── ml-service/                             # Python Flask Machine Learning API (Port 5000)
│       └── model_artifacts/                    # Serialized models, encoders, & pipeline config
│           ├── winning_model.pkl               # Serialized Cost-Aware XGBoost Model
│           ├── category_encoder.pkl            # LabelEncoder for 72 product categories
│           ├── feature_names.pkl               # 27 model input feature definitions
│           ├── pipeline_config.json            # Ground truth threshold & loss metadata
│           └── category_return_rates.json      # OOF baseline return rates per category
│
└── README.md                                   # Repository setup & execution manual (This file)
```

---

##  Part 1: Data Science & Machine Learning Pipeline

The `data pipeline/` directory contains 9 sequential Jupyter Notebooks executing the CRISP-DM research methodology:

### Notebook Execution Steps:

1. **`Step 1` & `Step 2` (Data Integration & Merging):** Ingests raw Olist Brazilian E-Commerce dataset (99,441 unique orders, 112,650 items across 72 categories) and cleans missing values.
2. **`Step 3` (Feature Engineering):** Computes physical transport features:
   - **Volumetric Weight ($\text{kg}$):** $(\text{Length} \times \text{Width} \times \text{Height}) / 6000$
   - **Haversine Distance ($\text{km}$):** Spherical distance from customer coordinate to warehouse origin.
   - **Reviewer Deviance Score:** $|\text{Customer Review} - \text{Category Avg Review}|$.
3. **`Step 4` (Data Leakage Prevention):** Replaces static group-by aggregations with chronological rolling cumulative sums, eliminating artificial $0.99$ target correlation.
4. **`Step 5` (Exploratory Data Analysis):** Identifies $15.62\%$ baseline return rate and class imbalance ratio ($1 : 5.4$).
5. **`Step 6` (Cost Matrix Formulation):** Calculates instance-level cost weights:
   $$\text{Cost}_{FN} = \text{Price} + \text{Return Freight} + \text{R\$ 5.00 Handling}$$
   $$\text{Cost}_{FP} = \text{Price} \times 0.20 \text{ (Profit Margin)}$$
   $$\text{Sample Weight} = \frac{\text{Cost}_{FN}}{\text{Cost}_{FP}}$$
6. **`Step 7` & `Step 8` (Model Training & Cost-Sensitive Optimization):** Evaluates baseline Random Forest models and cost-sensitive XGBoost models across dynamic decision thresholds ($0.05$ to $0.95$).
7. **`Step 9.1` – `Step 9.5` (Evaluation, Tri-Tier Risk Triaging & Production Serialization):**
   - Evaluates Master 6-Model financial loss matrix.
   - Validates 5-fold cross-validation stability (AUC = 80.39% ± 0.28%).
   - Computes dynamic risk tier cutoffs (Green Tier $p < 0.21$, Yellow Tier $0.21 \le p < 0.61$, Red Tier $p \ge 0.61$).
   - **`Step 9.5`:** Serializes production model objects (`winning_model.pkl`, `category_encoder.pkl`, `feature_names.pkl`, `pipeline_config.json`, `category_return_rates.json`) directly into `ml-service/model_artifacts/` for real-time inference deployment. Validates 5-fold cross-validation stability ($\text{AUC} = 80.39\% \pm 0.28\%$), generates ROI figures, and serializes production model artifacts to `ml-service/model_artifacts/`.

---

##  Part 2: 3-Tier Enterprise Web Application

The `reverse-logistics-csml/` directory contains the production 3-tier system:

```
+-------------------------------------------------------------------+
|                    Angular 17 SPA Frontend                        |
|                     (http://localhost:4200)                       |
+-------------------------------------------------------------------+
                                 | (HTTP / CORS)
                                 v
+-------------------------------------------------------------------+
|                    .NET 8 Web API Backend                         |
|                     (http://localhost:5265)                       |
+-------------------------------------------------------------------+
                   |                               | (HTTP JSON)
                   v                               v
    +-----------------------------+ +-------------------------------+
    |  Microsoft SQL Server DB    | | Python Flask ML Microservice  |
    |  (Orders & Predictions Table| |    (http://localhost:5000)    |
    +-----------------------------+ +-------------------------------+
```

---

##  How to Run the Project (Step-by-Step Guide)

### Prerequisites

Ensure you have the following software installed:
- **Python 3.10+** (with `pip`)
- **Node.js 18+** & **npm**
- **.NET 8.0 SDK**
- **Microsoft SQL Server** (or SQL Server Express / LocalDB)

---

### Step 1: Start the Python Flask Machine Learning Service

1. Navigate to the ML service directory:
   ```bash
   cd reverse-logistics-csml/ml-service
   ```

2. Create and activate a Python virtual environment:
   ```bash
   python -m venv venv
   # On Windows PowerShell:
   .\venv\Scripts\Activate.ps1
   # On Linux/macOS:
   source venv/bin/activate
   ```

3. Install Python dependencies:
   ```bash
   pip install -r requirements.txt
   ```
   *(Required packages: `flask`, `flask-cors`, `xgboost`, `scikit-learn`, `pandas`, `numpy`, `joblib`)*

4. Launch the ML microservice:
   ```bash
   python app.py
   ```
   *The Flask ML microservice will start running on **`http://localhost:5000`**.*

---

### Step 2: Start the .NET 8 Web API Backend

1. Navigate to the backend service directory:
   ```bash
   cd reverse-logistics-csml/backend-service
   ```

2. Configure database connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=ReverseLogisticsDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. Apply database migrations / create database tables:
   ```bash
   dotnet ef database update
   ```

4. Launch the .NET 8 API:
   ```bash
   dotnet run
   ```
   *The .NET Web API will start running on **`http://localhost:5265`**.*

---

### Step 3: Start the Angular 17 Frontend Application

1. Navigate to the frontend service directory:
   ```bash
   cd reverse-logistics-csml/frontend-service
   ```

2. Install Angular dependencies:
   ```bash
   npm install
   ```

3. Start the Angular development server:
   ```bash
   ng serve
   # or: npm start
   ```

4. Open your browser and navigate to:
   ```
   http://localhost:4200
   ```

---

##  Interactive Order Prediction Workstation Usage

Once all three services are running:
1. Access the **Real-Time Order Prediction Workstation** at `http://localhost:4200`.
2. Input custom transaction parameters:
   - **Product Category** (e.g., `watches_gifts`, `telephony`, `office_furniture`)
   - **Product Price (R\$)** & **Freight Value (R\$)**
   - **Dimensions (L × W × H cm)** & **Weight (g)**
   - **Customer City & State**
3. Click **Evaluate Return Risk**.
4. The system will:
   - Calculate Volumetric Weight `(L × W × H) / 6000` and Haversine shipping distance.
   - Invoke Flask ML Service prediction endpoint (`http://localhost:5000/predict`).
   - Dynamically calculate $C_{FN}$ and $C_{FP}$ financial loss exposure.
   - Display the **Return Risk Probability %**, **Risk Tier Badge (Green / Yellow / Red)**, and **Recommended Operational Action**.
   - Persist historical prediction audits into the SQL database.

---

##  Summary Benchmark Performance

| Model Variant | Cutoff ($t$) | Accuracy | Recall | Total Financial Loss (Test Set) | Savings vs Baseline |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Model 1: Cost-Unaware RF** | $0.50$ | $89.97\%$ | $45.11\%$ | R\$ 106,756.55 | Baseline |
| **Model 4: Cost-Unaware XGBoost** | $0.50$ | $90.06\%$ | $45.20\%$ | R\$ 106,343.49 | R\$ 413.06 |
| **Model 6: Winning Cost-Aware XGBoost** | **$0.61$** | **$89.44\%$** | **$50.23\%$** | **R\$ 103,602.45** | **R\$ 4,002.60 (Test Set)** |

* **Projected Net Dataset Savings:** **R\$ 41,567.95** (**42.82% cost reduction** vs Static Ship-All Policy).

---

##  License & Citation

This project was developed for the MSc Data Science Degree Module `7CS041` at the University of Wolverhampton. All dataset rights belong to Olist and Kaggle.
