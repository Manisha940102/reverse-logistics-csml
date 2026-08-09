# Cost-Sensitive XGBoost ML Microservice

Flask microservice hosting the winning **Cost-Aware XGBoost model (Variant 3, $t=0.65$)** serialized from Step 9.5 of the data pipeline.

## Running the Service
```bash
python app.py
```
Service runs on `http://localhost:5000`

## Endpoints
- `GET /health` : Service health check & loaded model metadata
- `POST /predict` : Accepts order features + cost matrix config, returns predicted return probability, tri-tier risk classification, FN cost, FP cost, and recommended action.
