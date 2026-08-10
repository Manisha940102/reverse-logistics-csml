import os
import sys
import json
import pickle
import numpy as np
import pandas as pd
from flask import Flask, request, jsonify
from flask_cors import CORS

app = Flask(__name__)
CORS(app)

# Support running as a bundled PyInstaller EXE or raw Python script
if getattr(sys, 'frozen', False):
    BASE_DIR = os.path.dirname(sys.executable)
    local_art_dir = os.path.join(BASE_DIR, 'model_artifacts')
    if os.path.exists(local_art_dir):
        ARTIFACT_DIR = local_art_dir
    else:
        ARTIFACT_DIR = os.path.join(getattr(sys, '_MEIPASS', BASE_DIR), 'model_artifacts')
else:
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    ARTIFACT_DIR = os.path.join(BASE_DIR, 'model_artifacts')

MODEL_PATH = os.path.join(ARTIFACT_DIR, 'winning_model.pkl')
ENCODER_PATH = os.path.join(ARTIFACT_DIR, 'category_encoder.pkl')
FEATURES_PATH = os.path.join(ARTIFACT_DIR, 'feature_names.pkl')
CONFIG_PATH = os.path.join(ARTIFACT_DIR, 'pipeline_config.json')
CAT_RATES_PATH = os.path.join(ARTIFACT_DIR, 'category_return_rates.json')

# Load production artifacts on startup
print("======================================================================")
print("LOADING COST-SENSITIVE XGBOOST ML MODEL ARTIFACTS...")
print("======================================================================")

with open(MODEL_PATH, 'rb') as f:
    model = pickle.load(f)
print(f"  [OK] Winning Model Loaded ({type(model).__name__})")

with open(ENCODER_PATH, 'rb') as f:
    encoder = pickle.load(f)
print(f"  [OK] Category Encoder Loaded ({len(encoder.classes_)} classes)")

with open(FEATURES_PATH, 'rb') as f:
    feature_names = pickle.load(f)
print(f"  [OK] Feature Names Loaded ({len(feature_names)} features)")

with open(CONFIG_PATH, 'r', encoding='utf-8') as f:
    default_config = json.load(f)
print(f"  [OK] Pipeline Config Loaded (Optimal t = {default_config.get('optimal_decision_threshold')})")

category_return_rates = {}
if os.path.exists(CAT_RATES_PATH):
    with open(CAT_RATES_PATH, 'r', encoding='utf-8') as f:
        category_return_rates = json.load(f)
    print(f"  [OK] Category Return Rates Map Loaded ({len(category_return_rates)} categories)")

print("======================================================================\n")

@app.route('/health', methods=['GET'])
def health():
    return jsonify({
        "status": "healthy",
        "service": "Cost-Sensitive XGBoost ML Microservice",
        "model_loaded": True,
        "features_count": len(feature_names),
        "categories_mapped": len(category_return_rates),
        "optimal_threshold": default_config.get('optimal_decision_threshold', 0.65)
    })

@app.route('/predict', methods=['POST'])
def predict():
    try:
        data = request.get_json(force=True)
        if not data:
            return jsonify({"error": "No JSON payload received"}), 400

        features_input = data.get('features', {})
        config_input = data.get('config', {})

        cat_str = str(features_input.get('product_category_name_english', features_input.get('productCategory', 'unknown'))).strip().lower()

        # Parse pricing & freight parameters
        price = float(features_input.get('price', features_input.get('product_price', 0.0)))
        freight_value = float(features_input.get('freight_value', features_input.get('return_freight', 0.0)))

        weight_g = float(features_input.get('product_weight_g', 500.0))
        length_cm = float(features_input.get('product_length_cm', 20.0))
        height_cm = float(features_input.get('product_height_cm', 15.0))
        width_cm = float(features_input.get('product_width_cm', 10.0))
        photos_qty = float(features_input.get('product_photos_qty', 2.0))

        volume_cm3 = float(features_input.get('product_volume_cm3', length_cm * height_cm * width_cm))
        if volume_cm3 <= 0:
            volume_cm3 = max(1.0, length_cm * height_cm * width_cm)

        density_g_cm3 = float(features_input.get('density_g_cm3', weight_g / volume_cm3))
        shipping_cost_ratio = float(features_input.get('shipping_cost_ratio', (freight_value / price) if price > 0 else 0.0))
        freight_to_price_ratio = float(features_input.get('freight_to_price_ratio', shipping_cost_ratio))
        is_shipping_more_than_item = float(features_input.get('is_shipping_more_than_item', 1.0 if freight_value > price else 0.0))

        state_str = str(features_input.get('customer_state', '')).strip().upper()
        default_dist = 15.0 if state_str in ['SP', 'RJ', 'MG', ''] else 480.0
        distance_km = float(features_input.get('haversine_distance_km', features_input.get('shipping_distance_km', default_dist)))
        deviance_score = float(features_input.get('reviewer_deviance_score', features_input.get('reviewer_deviance', 0.0)))
        is_extreme = float(features_input.get('is_extreme_reviewer', 1.0 if abs(deviance_score) > 2.0 else 0.0))

        # Long-distance + high shipping cost ratio custom orders (e.g. Manaus, AM with Freight > 3x Item Price) carry elevated logistics delay risk
        default_delay = 5.0 if (distance_km > 2000.0 and shipping_cost_ratio > 3.0) else 0.0
        delay_days = float(features_input.get('delivery_delay_days', default_delay))

        customer_orders = float(features_input.get('customer_order_count', 0.0))
        customer_avg_rev = float(features_input.get('customer_avg_review', 5.0))
        customer_spend = float(features_input.get('customer_total_spend', price))

        # Dynamic category return rate lookup
        default_cat_rate = category_return_rates.get(cat_str, 0.1507)
        category_ret_rate = float(features_input.get('category_return_rate', default_cat_rate))
        product_ret_rate = float(features_input.get('product_return_rate', default_cat_rate))
        customer_ret_rate = float(features_input.get('customer_return_rate', 0.0))
        product_sales = float(features_input.get('product_total_sales', 50.0))

        cat_str = str(features_input.get('product_category_name_english', features_input.get('productCategory', 'unknown'))).strip().lower()

        # Handle category label encoding
        try:
            category_encoded = int(encoder.transform([cat_str])[0])
        except Exception:
            category_encoded = 0

        # Construct exact 24-feature dictionary matching feature_names.pkl
        row_dict = {
            'price': price,
            'freight_value': freight_value,
            'shipping_cost_ratio': shipping_cost_ratio,
            'is_shipping_more_than_item': is_shipping_more_than_item,
            'freight_to_price_ratio': freight_to_price_ratio,
            'product_weight_g': weight_g,
            'product_length_cm': length_cm,
            'product_height_cm': height_cm,
            'product_width_cm': width_cm,
            'product_volume_cm3': volume_cm3,
            'product_photos_qty': photos_qty,
            'density_g_cm3': density_g_cm3,
            'delivery_delay_days': delay_days,
            'customer_order_count': customer_orders,
            'customer_avg_review': customer_avg_rev,
            'customer_return_rate': customer_ret_rate,
            'customer_total_spend': customer_spend,
            'is_extreme_reviewer': is_extreme,
            'reviewer_deviance_score': deviance_score,
            'product_return_rate': product_ret_rate,
            'product_total_sales': product_sales,
            'category_return_rate': category_ret_rate,
            'haversine_distance_km': distance_km,
            'category_encoded': category_encoded
        }

        X_df = pd.DataFrame([row_dict])[feature_names]

        # Model Inference
        probability = float(model.predict_proba(X_df)[0, 1])

        # Dynamic Threshold & Config Parameters from DB payload or default
        profit_margin = float(config_input.get('ProfitMarginPercentage', 0.20))
        handling_cost = float(config_input.get('HandlingCostPerOrder', 5.00))
        t_high = float(config_input.get('DynamicThreshold', default_config.get('optimal_decision_threshold', 0.65)))
        t_low = float(config_input.get('GreenThreshold', default_config.get('t_low_green_threshold', 0.29)))

        # Determine Tri-Tier Risk Classification (with Freight > Price Red & Low-Freight Green Overrides)
        is_low_risk_local = (shipping_cost_ratio <= 0.05 and photos_qty >= 3 and price >= 100.0)
        
        if is_shipping_more_than_item == 1.0 or probability >= t_high:
            risk_category = "Red"
            recommended_action = "High-Priority Return Mitigation"
        elif probability < t_low or (is_low_risk_local and probability < 0.30):
            risk_category = "Green"
            recommended_action = "Standard Automated Dispatch"
        else:
            risk_category = "Yellow"
            recommended_action = "Targeted Pre-Dispatch Intervention"

        # Financial Loss Costs Calculation
        return_freight = float(features_input.get('return_freight', freight_value))
        fn_cost = freight_value + return_freight + handling_cost
        fp_cost = price * profit_margin

        return jsonify({
            "probability": round(probability, 4),
            "risk_category": risk_category,
            "fn_cost": round(fn_cost, 2),
            "fp_cost": round(fp_cost, 2),
            "recommended_action": recommended_action,
            "threshold_used": round(t_high, 2)
        })

    except Exception as e:
        app.logger.error(f"Prediction failed: {str(e)}", exc_info=True)
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    print("Starting Flask ML Service on http://0.0.0.0:5000 ...")
    app.run(host='0.0.0.0', port=5000, debug=False)
