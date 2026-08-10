# What-If Sensitivity Simulation Scenarios

This directory contains standalone execution scripts and exported benchmarks for evaluating return risk, tri-tier operational risk triaging (Green, Yellow, Red), and financial loss exposure across pre-configured **"What-If" sensitivity scenarios**.

## 1. Executive Summary of Scenarios

The simulations evaluate the Cost-Sensitive XGBoost model across 5 representative operational conditions:

| Scenario | Category | Price (R$) | Freight (R$) | Distance (km) | Reviewer Deviance | Return Prob (%) | Risk Tier | Recommended Action |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Scenario 1: Low Risk Standard Order** | `health_beauty` | R$ 200.00 | R$ 8.00 | 15.0 km | 0.0 (Neutral) | **40.47%** | **Green Risk** | Standard Automated Dispatch |
| **Scenario 2: High Freight & Long Distance** | `fashion_bags_accessories` | R$ 30.00 | R$ 45.00 | 2689.4 km | 0.0 (Neutral) | **91.86%** | **Red Risk** | High-Priority Return Mitigation |
| **Scenario 3: Extreme Reviewer Behavior Test** | `health_beauty` | R$ 200.00 | R$ 8.00 | 15.0 km | 2.5 (Extreme) | **77.84%** | **Red Risk** | High-Priority Return Mitigation |
| **Scenario 4: Heavy Bulky Housewares Item** | `housewares` | R$ 150.00 | R$ 50.00 | 800.0 km | 0.5 (Mild) | **86.53%** | **Red Risk** | High-Priority Return Mitigation |
| **Scenario 5: Pre-Purchase Neutral Imputation** | `computers_accessories` | R$ 450.00 | R$ 25.00 | 350.0 km | 0.0 (Default) | **66.97%** | **Red Risk** | High-Priority Return Mitigation |

---

## 2. Key Findings & Insights

1. **Impact of Reviewer Deviance (Sensitivity Test)**:
   Comparing **Scenario 1** (Deviance = 0.0) with **Scenario 3** (Deviance = 2.5):
   * Holding all physical, pricing, and spatial parameters constant, introducing extreme reviewer behavior (`is_extreme_reviewer = 1.0`) increases predicted return probability from **40.47% to 77.84%**, elevating the risk classification from **Green Tier to Red Tier**.

2. **Freight-to-Price Ratio Penalty**:
   In **Scenario 2**, when freight cost (`R$ 45.00`) exceeds product price (`R$ 30.00`), the cost-sensitive policy automatically triggers **Red Tier High-Priority Mitigation** due to severe asymmetric False Negative financial exposure (`FN Cost = R$ 104.00` vs `FP Cost = R$ 6.00`).

3. **Pre-Purchase Neutral Imputation**:
   For cold-start / pre-purchase workstation simulations (**Scenario 5**), missing customer behavioral features default safely to neutral baseline (`0.0`), enabling accurate prediction based on item volumetrics, pricing, and spatial distance.

---

## 3. How to Run the What-If Script

Run the python script directly from the project root directory:

```bash
python what_if_scenarios.py
```

Outputs will be printed to stdout and exported to `what_if_scenarios_results.csv`.
