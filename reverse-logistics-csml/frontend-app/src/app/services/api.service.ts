import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  OrderDetail,
  PredictionResponse,
  CostMatrixConfig,
  UpdateConfigRequest,
  ModelComparison,
  PredictionsSummary,
  PredictionHistoryItem,
} from '../models/interfaces';

/**
 * Centralized API service for all backend communication.
 * Base URL points to the .NET 8 Web API at localhost:5139.
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = 'http://localhost:5099/api';

  constructor(private http: HttpClient) {}

  // ── Orders ──

  /** GET /api/orders/pending?page=&pageSize= */
  getPendingOrders(page = 1, pageSize = 20): Observable<OrderDetail[]> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<OrderDetail[]>(`${this.baseUrl}/orders/pending`, { params });
  }

  /** POST /api/orders/evaluate/{orderId} */
  evaluateOrder(orderId: string): Observable<PredictionResponse> {
    return this.http.post<PredictionResponse>(
      `${this.baseUrl}/orders/evaluate/${encodeURIComponent(orderId)}`,
      {}
    );
  }

  /** POST /api/orders/evaluate-custom */
  evaluateCustomOrder(payload: any): Observable<PredictionResponse> {
    return this.http.post<PredictionResponse>(
      `${this.baseUrl}/orders/evaluate-custom`,
      payload
    );
  }

  // ── Management / CRUD ──

  getCustomers(page = 1, pageSize = 50, search?: string): Observable<any> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (search) {
      params = params.set('search', search);
    }
      
    return this.http.get<any>(`${this.baseUrl}/customers`, { params });
  }

  getCustomerCities(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/customers/cities`);
  }

  getCustomerStates(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/customers/states`);
  }

  getGeolocations(page = 1, pageSize = 50, search?: string): Observable<any> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);
    return this.http.get<any>(`${this.baseUrl}/geolocations`, { params });
  }

  getOrders(page = 1, pageSize = 50, search?: string): Observable<any> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);
    return this.http.get<any>(`${this.baseUrl}/orders`, { params });
  }

  getOrderItems(page = 1, pageSize = 50, search?: string): Observable<any> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);
    return this.http.get<any>(`${this.baseUrl}/orderitems`, { params });
  }

  getOrderReviews(page = 1, pageSize = 50, search?: string): Observable<any> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);
    return this.http.get<any>(`${this.baseUrl}/orderreviews`, { params });
  }

  getProducts(page = 1, pageSize = 50, search?: string): Observable<any> {
    let params = new HttpParams().set('page', page.toString()).set('pageSize', pageSize.toString());
    if (search) params = params.set('search', search);
    return this.http.get<any>(`${this.baseUrl}/products`, { params });
  }

  getProductCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/products/categories`);
  }

  // ── Configuration ──

  /** GET /api/configuration */
  getConfig(): Observable<CostMatrixConfig> {
    return this.http.get<CostMatrixConfig>(`${this.baseUrl}/configuration`);
  }

  /** PUT /api/configuration */
  updateConfig(request: UpdateConfigRequest): Observable<CostMatrixConfig> {
    return this.http.put<CostMatrixConfig>(`${this.baseUrl}/configuration`, request);
  }

  // ── Analytics ──

  /** GET /api/analytics/model-comparison */
  getModelComparison(): Observable<ModelComparison[]> {
    return this.http.get<ModelComparison[]>(`${this.baseUrl}/analytics/model-comparison`);
  }

  /** GET /api/analytics/predictions-summary */
  getPredictionsSummary(): Observable<PredictionsSummary> {
    return this.http.get<PredictionsSummary>(`${this.baseUrl}/analytics/predictions-summary`);
  }

  /** GET /api/analytics/predictions-history?page=&pageSize= */
  getPredictionsHistory(page = 1, pageSize = 20): Observable<PredictionHistoryItem[]> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<PredictionHistoryItem[]>(
      `${this.baseUrl}/analytics/predictions-history`,
      { params }
    );
  }
}
