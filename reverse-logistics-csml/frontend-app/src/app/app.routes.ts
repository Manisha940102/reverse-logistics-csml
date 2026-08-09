import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'predict',
    pathMatch: 'full',
  },
  {
    path: 'predict',
    loadComponent: () =>
      import('./components/prediction-workstation/prediction-workstation.component').then(
        (m) => m.PredictionWorkstationComponent
      ),
  },
  {
    path: 'benchmarks',
    loadComponent: () =>
      import('./components/model-benchmark/model-benchmark.component').then(
        (m) => m.ModelBenchmarkComponent
      ),
  },
  {
    path: 'configuration',
    loadComponent: () =>
      import('./components/configuration/configuration.component').then(
        (m) => m.ConfigurationComponent
      ),
  },
  {
    path: 'history',
    loadComponent: () =>
      import('./components/prediction-history/prediction-history.component').then(
        (m) => m.PredictionHistoryComponent
      ),
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./components/customers/customers.component').then((m) => m.CustomerComponent),
  },
  {
    path: 'geolocations',
    loadComponent: () =>
      import('./components/geolocations/geolocations.component').then((m) => m.GeolocationComponent),
  },
  {
    path: 'orders-management',
    loadComponent: () =>
      import('./components/orders-management/orders-management.component').then((m) => m.OrderComponent),
  },
  {
    path: 'order-items',
    loadComponent: () =>
      import('./components/order-items/order-items.component').then((m) => m.OrderItemComponent),
  },
  {
    path: 'order-reviews',
    loadComponent: () =>
      import('./components/order-reviews/order-reviews.component').then((m) => m.OrderReviewComponent),
  },
  {
    path: 'products',
    loadComponent: () =>
      import('./components/products/products.component').then((m) => m.ProductComponent),
  },
  {
    path: '**',
    redirectTo: 'predict',
  },
];
