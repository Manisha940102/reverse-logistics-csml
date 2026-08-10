"""
What-If Simulation Scenarios Script
====================================
This script executes pre-configured 'What-If' sensitivity scenarios against the 
serialized Cost-Sensitive XGBoost ML model to evaluate return risk probability,
tri-tier risk classification (Green, Yellow, Red), and financial loss exposure.

Run: python what_if_scenarios.py
"""

import os
import sys
import json
import pickle
import numpy as np
import pandas as pd

# Load ML Service artifacts
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
ARTIFACTS_DIR = os.path.join(BASE_DIR, 'model_artifacts')

if not os.path.exists(ARTIFACTS_DIR):
    # Try root or step 9 directory fallback
    alt_dir = os.path.join(os.path.dirname(BASE_DIR), 'data pipeline', 'Step 9 - Final Evaluation & Financial Impact (ROI)', 'pipeline_artifacts')
    if os.path.exists(alt_dir):
        ARTIFACTS_DIR = alt_dir

model_path = os.path.join(ARTIFACTS_DIR, 'winning_model.pkl')
encoder_path = os.path.join(ARTIFACTS_DIR, 'category_encoder.pkl')
features_path = os.path.join(ARTIFACTS_DIR, 'feature_names.pkl')
config_json_path = os.path.join(ARTIFACTS_DIR, 'pipeline_config.json')

with open(model_path, 'rb') as f:
    model = pickle.load(f)

with open(encoder_path, 'rb') as f:
    category_encoder = pickle.load(f)

with open(features_path, 'rb') as f:
    feature_names = pickle.load(f)

pipeline_config = {
    'selected_winning_model': 'Variant 3: Cost-Aware XGBoost + OOF Threshold',
    'optimal_decision_threshold': 0.61,
    't_low_green_threshold': 0.20,
    't_high_red_threshold': 0.65
}

if os.path.exists(config_json_path):
    with open(config_json_path, 'r', encoding='utf-8') as f:
        pipeline_config = json.load(f)


def build_feature_vector(input_dict):
    """Engineers all 24 required ML feature variables from input payload."""
    price = float(input_dict.get('price', input_dict.get('product_price', 0.0)))
    freight_value = float(input_dict.get('freight_value', 0.0))

    shipping_cost_ratio = float(input_dict.get('shipping_cost_ratio', (freight_value / price) if price > 0 else 0.0))
    freight_to_price_ratio = float(input_dict.get('freight_to_price_ratio', shipping_cost_ratio))
    is_shipping_more_than_item = float(input_dict.get('is_shipping_more_than_item', 1.0 if freight_value > price else 0.0))

    product_weight_g = float(input_dict.get('product_weight_g', 500.0))
    product_length_cm = float(input_dict.get('product_length_cm', 20.0))
    product_height_cm = float(input_dict.get('product_height_cm', 15.0))
    product_width_cm = float(input_dict.get('product_width_cm', 15.0))

    computed_volume = product_length_cm * product_height_cm * product_width_cm
    product_volume_cm3 = float(input_dict.get('product_volume_cm3', computed_volume if computed_volume > 0 else 4500.0))

    product_photos_qty = float(input_dict.get('product_photos_qty', 2.0))
    density_g_cm3 = float(input_dict.get('density_g_cm3', (product_weight_g / product_volume_cm3) if product_volume_cm3 > 0 else 0.1))

    delivery_delay_days = float(input_dict.get('delivery_delay_days', 0.0))

    customer_order_count = float(input_dict.get('customer_order_count', 1.0))
    customer_avg_review = float(input_dict.get('customer_avg_review', 4.0))
    customer_return_rate = float(input_dict.get('customer_return_rate', 0.1562))
    customer_total_spend = float(input_dict.get('customer_total_spend', price + freight_value))

    reviewer_deviance_score = float(input_dict.get('reviewer_deviance_score', input_dict.get('reviewer_deviance', 0.0)))
    is_extreme_reviewer = float(input_dict.get('is_extreme_reviewer', 1.0 if abs(reviewer_deviance_score) > 2.0 else 0.0))

    product_return_rate = float(input_dict.get('product_return_rate', 0.1562))
    product_total_sales = float(input_dict.get('product_total_sales', 10.0))
    category_return_rate = float(input_dict.get('category_return_rate', 0.1562))

    haversine_distance_km = float(input_dict.get('haversine_distance_km', input_dict.get('shipping_distance_km', 500.0)))

    raw_category = str(input_dict.get('product_category_name_english', 'unknown')).strip().lower()
    encoded_classes = [c.lower() for c in category_encoder.classes_]

    if raw_category in encoded_classes:
        category_encoded = int(np.where(np.array(encoded_classes) == raw_category)[0][0])
    else:
        category_encoded = int(np.where(np.array(encoded_classes) == 'unknown')[0][0]) if 'unknown' in encoded_classes else 0

    feature_map = {
        'price': price,
        'freight_value': freight_value,
        'shipping_cost_ratio': shipping_cost_ratio,
        'is_shipping_more_than_item': is_shipping_more_than_item,
        'freight_to_price_ratio': freight_to_price_ratio,
        'product_weight_g': product_weight_g,
        'product_length_cm': product_length_cm,
        'product_height_cm': product_height_cm,
        'product_width_cm': product_width_cm,
        'product_volume_cm3': product_volume_cm3,
        'product_photos_qty': product_photos_qty,
        'density_g_cm3': density_g_cm3,
        'delivery_delay_days': delivery_delay_days,
        'customer_order_count': customer_order_count,
        'customer_avg_review': customer_avg_review,
        'customer_return_rate': customer_return_rate,
        'customer_total_spend': customer_total_spend,
        'is_extreme_reviewer': is_extreme_reviewer,
        'reviewer_deviance_score': reviewer_deviance_score,
        'product_return_rate': product_return_rate,
        'product_total_sales': product_total_sales,
        'category_return_rate': category_return_rate,
        'haversine_distance_km': haversine_distance_km,
        'category_encoded': category_encoded
    }

    df_features = pd.DataFrame([feature_map])[feature_names]
    return df_features, feature_map


def run_scenario(scenario_name, input_dict):
    """Executes prediction and risk classification for a single scenario."""
    df_X, feature_map = build_feature_vector(input_dict)
    probabilities = model.predict_proba(df_X)
    probability = float(probabilities[0, 1])

    t_high = float(pipeline_config.get('optimal_decision_threshold', 0.61))
    t_low = float(pipeline_config.get('t_low_green_threshold', 0.20))

    profit_margin = 0.20
    handling_cost = 5.0

    freight_value = feature_map['freight_value']
    price = feature_map['price']
    is_shipping_more_than_item = feature_map['is_shipping_more_than_item']
    shipping_cost_ratio = feature_map['shipping_cost_ratio']

    return_freight = freight_value * 1.2
    fn_cost = freight_value + return_freight + handling_cost
    fp_cost = price * profit_margin

    if is_shipping_more_than_item == 1.0 or probability >= t_high:
        risk_category = "Red"
        recommended_action = "High-Priority Return Mitigation"
    elif probability < t_low or shipping_cost_ratio <= 0.05:
        risk_category = "Green"
        recommended_action = "Standard Automated Dispatch"
    else:
        risk_category = "Yellow"
        recommended_action = "Targeted Pre-Dispatch Intervention"

    return {
        "Scenario": scenario_name,
        "Category": input_dict.get('product_category_name_english', 'unknown'),
        "Price (R$)": price,
        "Freight (R$)": freight_value,
        "Weight (g)": feature_map['product_weight_g'],
        "Distance (km)": feature_map['haversine_distance_km'],
        "Reviewer Deviance": feature_map['reviewer_deviance_score'],
        "Is Extreme Reviewer": feature_map['is_extreme_reviewer'],
        "Return Prob (%)": round(probability * 100, 2),
        "Risk Tier": risk_category,
        "FN Cost (R$)": round(fn_cost, 2),
        "FP Cost (R$)": round(fp_cost, 2),
        "Recommended Action": recommended_action
    }


def main():
    scenarios = [
        ("Scenario 1: Low Risk Standard Order (Green Tier)", {
            "product_category_name_english": "health_beauty",
            "price": 200.0,
            "freight_value": 8.0,
            "product_weight_g": 300.0,
            "product_length_cm": 15.0,
            "product_height_cm": 10.0,
            "product_width_cm": 10.0,
            "product_photos_qty": 4,
            "shipping_distance_km": 15.0,
            "reviewer_deviance": 0.0
        }),
        ("Scenario 2: High Freight & Long Distance (Red Risk Tier)", {
            "product_category_name_english": "fashion_bags_accessories",
            "price": 30.0,
            "freight_value": 45.0,
            "product_weight_g": 4500.0,
            "product_length_cm": 50.0,
            "product_height_cm": 40.0,
            "product_width_cm": 35.0,
            "product_photos_qty": 1,
            "shipping_distance_km": 2689.4,
            "reviewer_deviance": 0.0
        }),
        ("Scenario 3: Extreme Reviewer Behavior Sensitivity Test", {
            "product_category_name_english": "health_beauty",
            "price": 200.0,
            "freight_value": 8.0,
            "product_weight_g": 300.0,
            "product_length_cm": 15.0,
            "product_height_cm": 10.0,
            "product_width_cm": 10.0,
            "product_photos_qty": 4,
            "shipping_distance_km": 15.0,
            "reviewer_deviance": 2.5
        }),
        ("Scenario 4: Heavy Bulky Housewares Item (Yellow Risk Tier)", {
            "product_category_name_english": "housewares",
            "price": 150.0,
            "freight_value": 50.0,
            "product_weight_g": 8000.0,
            "product_length_cm": 60.0,
            "product_height_cm": 40.0,
            "product_width_cm": 40.0,
            "product_photos_qty": 2,
            "shipping_distance_km": 800.0,
            "reviewer_deviance": 0.5
        }),
        ("Scenario 5: Pre-Purchase Neutral Imputation (Default 0.0)", {
            "product_category_name_english": "computers_accessories",
            "price": 450.0,
            "freight_value": 25.0,
            "product_weight_g": 1200.0,
            "product_length_cm": 30.0,
            "product_height_cm": 20.0,
            "product_width_cm": 20.0,
            "product_photos_qty": 5,
            "shipping_distance_km": 350.0,
            "reviewer_deviance": 0.0
        })
    ]

    results = []
    print("=" * 80)
    print("COST-SENSITIVE MACHINE LEARNING - WHAT-IF SIMULATION SCENARIOS")
    print("=" * 80)
    
    for name, input_dict in scenarios:
        res = run_scenario(name, input_dict)
        results.append(res)
        print(f"\n--- {name} ---")
        for k, v in res.items():
            if k != "Scenario":
                print(f"  {k:25s}: {v}")

    df_results = pd.DataFrame(results)
    csv_out = os.path.join(BASE_DIR, 'what_if_scenarios_results.csv')
    df_results.to_csv(csv_out, index=False)
    print("\n" + "=" * 80)
    print(f"Results successfully exported to: {csv_out}")
    print("=" * 80)

if __name__ == '__main__':
    main()
