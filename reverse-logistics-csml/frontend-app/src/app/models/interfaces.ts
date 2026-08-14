/**
 * TypeScript interfaces for all API models.
 * Maps 1:1 with .NET DTOs and SQL Server tables.
 */

/** GET /api/orders/pending response item */
export interface OrderDetail {
  orderId: string;
  orderStatus: string;
  orderPurchaseTimestamp: string;
  customerCity: string | null;
  customerState: string | null;
  totalPrice: number;
  totalFreight: number;
  itemCount: number;
}

/** POST /api/orders/evaluate/{id} response */
export interface PredictionResponse {
  orderId: string;
  probability: number;
  riskCategory: 'Green' | 'Yellow' | 'Red';
  fnCost: number;
  fpCost: number;
  recommendedAction: string;
  thresholdUsed: number;
  latencyMs: number;
  productPrice: number;
  freightValue: number;
  productCategory: string | null;
  customerCity: string | null;
  customerState: string | null;
  shippingDistanceKm: number | null;
  volumetricWeight: number | null;
}

/** GET /api/config response */
export interface CostMatrixConfig {
  configId: number;
  profitMarginPercentage: number;
  handlingCostPerOrder: number;
  dynamicThreshold: number;
  activeStatus: boolean;
  updatedAt: string;
}

/** PUT /api/config request body */
export interface UpdateConfigRequest {
  profitMarginPercentage?: number;
  handlingCostPerOrder?: number;
  dynamicThreshold?: number;
}

/** GET /api/analytics/model-comparison response item */
export interface ModelComparison {
  modelId: number;
  modelName: string;
  classificationAccuracy: number;
  precisionScore: number;
  recallScore: number;
  f1Score: number;
  aucRoc: number;
  falseNegativesCount: number;
  falsePositivesCount: number;
  totalFinancialLoss: number;
  operationalSavings: number;
  isOptimal: boolean;
}

/** GET /api/analytics/predictions-summary response */
export interface PredictionsSummary {
  totalPredictions: number;
  greenCount: number;
  yellowCount: number;
  redCount: number;
  totalFnCost: number;
  totalFpCost: number;
}

/** Prediction history row (from /api/analytics/predictions-history) */
export interface PredictionHistoryItem {
  predictionId: number;
  orderId: string;
  returnProbability: number;
  riskCategory: 'Green' | 'Yellow' | 'Red';
  falseNegativeCost: number;
  falsePositiveCost: number;
  optimalThreshold: number;
  recommendedAction: string;
  createdAt: string;
}
