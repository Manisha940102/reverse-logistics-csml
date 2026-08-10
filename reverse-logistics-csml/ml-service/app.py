import os
import json
import pickle
import numpy as np
import pandas as pd
from flask import Flask, request, jsonify

app = Flask(__name__)

# Define base paths
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
ARTIFACTS_DIR = os.path.join(BASE_DIR, 'model_artifacts')

# Global variables for model artifacts
model = None
category_encoder = None
feature_names = None
pipeline_config = {}

def load_artifacts():
    """Load serialized ML model, category encoder, feature names, and pipeline configuration."""
    global model, category_encoder, feature_names, pipeline_config

    model_path = os.path.join(ARTIFACTS_DIR, 'winning_model.pkl')
    encoder_path = os.path.join(ARTIFACTS_DIR, 'category_encoder.pkl')
    features_path = os.path.join(ARTIFACTS_DIR, 'feature_names.pkl')
    config_json_path = os.path.join(ARTIFACTS_DIR, 'pipeline_config.json')

    # Fallback to pipeline_artifacts if model_artifacts is missing files
    if not os.path.exists(model_path):
        alt_artifacts = os.path.join(os.path.dirname(BASE_DIR), 'data pipeline', 'Step 9 - Final Evaluation & Financial Impact (ROI)', 'pipeline_artifacts')
        if os.path.exists(os.path.join(alt_artifacts, 'winning_model.pkl')):
            model_path = os.path.join(alt_artifacts, 'winning_model.pkl')
            encoder_path = os.path.join(alt_artifacts, 'category_encoder.pkl')
            features_path = os.path.join(alt_artifacts, 'feature_names.pkl')
            config_json_path = os.path.join(alt_artifacts, 'pipeline_config.json')

    print(f"Loading ML model from: {model_path}")
    with open(model_path, 'rb') as f:
        model = pickle.load(f)

    print(f"Loading Category Encoder from: {encoder_path}")
    with open(encoder_path, 'rb') as f:
        category_encoder = pickle.load(f)

    print(f"Loading Feature Names from: {features_path}")
    with open(features_path, 'rb') as f:
        feature_names = pickle.load(f)

    print(f"Loading Pipeline Config from: {config_json_path}")
    if os.path.exists(config_json_path):
        with open(config_json_path, 'r', encoding='utf-8') as f:
            pipeline_config = json.load(f)
    else:
        pipeline_config = {
            'selected_winning_model': 'Variant 3: Cost-Aware XGBoost + OOF Threshold',
            'optimal_decision_threshold': 0.61,
            't_low_green_threshold': 0.20,
            't_high_red_threshold': 0.65
        }

    print("All ML artifacts loaded successfully!")
    print(f" - Model Type: {type(model).__name__}")
    print(f" - Feature Count: {len(feature_names)}")
    print(f" - Categories Encoded: {len(category_encoder.classes_)}")

# Load artifacts on startup
load_artifacts()


def build_feature_vector(input_dict):
    """
    Dynamically engineer and format all 24 required ML feature variables from incoming payload.
    No hardcoded feature placeholders — computes volume, density, ratios, distance, and encodings dynamically.
    """
    # 1. Base pricing parameters
    price = float(input_dict.get('price', input_dict.get('product_price', 0.0)))
    freight_value = float(input_dict.get('freight_value', 0.0))

    # Derived financial ratios
    shipping_cost_ratio = float(input_dict.get('shipping_cost_ratio', (freight_value / price) if price > 0 else 0.0))
    freight_to_price_ratio = float(input_dict.get('freight_to_price_ratio', shipping_cost_ratio))
    is_shipping_more_than_item = float(input_dict.get('is_shipping_more_than_item', 1.0 if freight_value > price else 0.0))

    # 2. Package physical dimensions and volumetrics
    product_weight_g = float(input_dict.get('product_weight_g', 500.0))
    product_length_cm = float(input_dict.get('product_length_cm', 20.0))
    product_height_cm = float(input_dict.get('product_height_cm', 15.0))
    product_width_cm = float(input_dict.get('product_width_cm', 15.0))

    computed_volume = product_length_cm * product_height_cm * product_width_cm
    product_volume_cm3 = float(input_dict.get('product_volume_cm3', computed_volume if computed_volume > 0 else 4500.0))

    product_photos_qty = float(input_dict.get('product_photos_qty', 2.0))
    density_g_cm3 = float(input_dict.get('density_g_cm3', (product_weight_g / product_volume_cm3) if product_volume_cm3 > 0 else 0.1))

    # 3. Logistics tracking & delay
    delivery_delay_days = float(input_dict.get('delivery_delay_days', 0.0))

    # 4. Behavioral & reviewer deviance features
    customer_order_count = float(input_dict.get('customer_order_count', 1.0))
    customer_avg_review = float(input_dict.get('customer_avg_review', 4.0))
    customer_return_rate = float(input_dict.get('customer_return_rate', 0.1562))
    customer_total_spend = float(input_dict.get('customer_total_spend', price + freight_value))

    reviewer_deviance_score = float(input_dict.get('reviewer_deviance_score', input_dict.get('reviewer_deviance', 0.0)))
    is_extreme_reviewer = float(input_dict.get('is_extreme_reviewer', 1.0 if abs(reviewer_deviance_score) > 2.0 else 0.0))

    # 5. Domain category & product return baselines
    product_return_rate = float(input_dict.get('product_return_rate', 0.1562))
    product_total_sales = float(input_dict.get('product_total_sales', 10.0))
    category_return_rate = float(input_dict.get('category_return_rate', 0.1562))

    # 6. Spatial haversine distance
    haversine_distance_km = float(input_dict.get('haversine_distance_km', input_dict.get('shipping_distance_km', 500.0)))

    # 7. Category Label Encoding
    raw_category = str(input_dict.get('product_category_name_english', 'unknown')).strip().lower()
    encoded_classes = [c.lower() for c in category_encoder.classes_]

    if raw_category in encoded_classes:
        category_encoded = int(np.where(np.array(encoded_classes) == raw_category)[0][0])
    else:
        # Fallback to 'unknown' class or index 0
        if 'unknown' in encoded_classes:
            category_encoded = int(np.where(np.array(encoded_classes) == 'unknown')[0][0])
        else:
            category_encoded = 0

    # Build dictionary of all computed features
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

    # Format into DataFrame with exact feature ordering expected by serialized XGBoost model
    df_features = pd.DataFrame([feature_map])
    df_features = df_features[feature_names]

    return df_features, feature_map


@app.route('/', methods=['GET'])
def index():
    """API Root endpoint."""
    return jsonify({
        "service": "Cost-Sensitive Machine Learning Flask Service",
        "status": "online",
        "model": pipeline_config.get("selected_winning_model", "Cost-Aware XGBoost"),
        "optimal_threshold": pipeline_config.get("optimal_decision_threshold", 0.61)
    })


@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint."""
    return jsonify({
        "status": "healthy",
        "model_loaded": model is not None,
        "encoder_loaded": category_encoder is not None,
        "feature_count": len(feature_names) if feature_names else 0
    })


@app.route('/config', methods=['GET'])
def get_config():
    """Returns the loaded pipeline configuration."""
    return jsonify(pipeline_config)


@app.route('/predict', methods=['POST'])
def predict():
    """
    Main prediction endpoint. Accepts incoming order feature dict and returns:
    - Return probability (0.0 to 1.0)
    - Tri-Tier Risk Classification (Green, Yellow, Red)
    - Instance-level False Negative Cost (FN Cost)
    - Instance-level False Positive Cost (FP Cost)
    - Recommended operational action
    - Threshold utilized
    """
    try:
        payload = request.get_json(force=True) or {}

        # Handle nested {"features": {...}, "config": {...}} or flat JSON payload
        features_input = payload.get('features', payload)
        config_input = payload.get('config', {})

        # Extract features and format DataFrame
        df_X, feature_map = build_feature_vector(features_input)

        # Execute ML prediction using serialized XGBoost model
        probabilities = model.predict_proba(df_X)
        probability = float(probabilities[0, 1])

        # Parse threshold and cost configuration parameters
        t_high = float(config_input.get('DynamicThreshold', config_input.get('t_high_red_threshold', pipeline_config.get('optimal_decision_threshold', 0.61))))
        t_low = float(pipeline_config.get('t_low_green_threshold', 0.20))

        profit_margin = float(config_input.get('ProfitMarginPercentage', 20.0)) / 100.0
        handling_cost = float(config_input.get('HandlingCostPerOrder', 5.0))

        # Financial Loss Costs Calculation (Matching Step 6 Dynamic Cost Matrix)
        freight_value = feature_map['freight_value']
        price = feature_map['price']
        is_shipping_more_than_item = feature_map['is_shipping_more_than_item']
        shipping_cost_ratio = feature_map['shipping_cost_ratio']

        # Reverse Freight = 1.2x outbound freight
        return_freight = float(features_input.get('return_freight', freight_value * 1.2))
        fn_cost = freight_value + return_freight + handling_cost
        fp_cost = price * profit_margin

        # Tri-Tier Risk Stratification Framework (Matching Step 9.4 Policy)
        if is_shipping_more_than_item == 1.0 or probability >= t_high:
            risk_category = "Red"
            recommended_action = "High-Priority Return Mitigation"
        elif probability < t_low or shipping_cost_ratio <= 0.05:
            risk_category = "Green"
            recommended_action = "Standard Automated Dispatch"
        else:
            risk_category = "Yellow"
            recommended_action = "Targeted Pre-Dispatch Intervention"

        return jsonify({
            "probability": round(probability, 4),
            "risk_category": risk_category,
            "fn_cost": round(fn_cost, 2),
            "fp_cost": round(fp_cost, 2),
            "recommended_action": recommended_action,
            "threshold_used": round(t_high, 2)
        })

    except Exception as e:
        app.logger.error(f"Error during ML prediction: {str(e)}", exc_info=True)
        return jsonify({
            "error": str(e),
            "probability": 0.0,
            "risk_category": "Red",
            "fn_cost": 0.0,
            "fp_cost": 0.0,
            "recommended_action": "Error in ML Inference - Review Payload",
            "threshold_used": 0.61
        }), 500


if __name__ == '__main__':
    port = int(os.environ.get('PORT', 5000))
    print(f"Starting Flask ML Service on port {port}...")
    app.run(host='0.0.0.0', port=port, debug=False)
