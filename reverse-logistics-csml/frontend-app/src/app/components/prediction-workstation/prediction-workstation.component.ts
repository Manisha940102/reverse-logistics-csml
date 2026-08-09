import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { OrderDetail, PredictionResponse } from '../../models/interfaces';

@Component({
  selector: 'app-prediction-workstation',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './prediction-workstation.component.html',
  styleUrl: './prediction-workstation.component.css',
})
export class PredictionWorkstationComponent implements OnInit {
  customOrder: any = {
    productCategory: '',
    productPrice: null,
    freightValue: null,
    productWeightG: null,
    productLengthCm: null,
    productHeightCm: null,
    productWidthCm: null,
    productPhotosQty: null,
    customerCity: '',
    customerState: ''
  };

  prediction: PredictionResponse | null = null;
  loading = false;
  errorMessage = '';

  categories: string[] = [];
  filteredCategories: string[] = [];
  showDropdown = false;

  cities: string[] = [];
  filteredCities: string[] = [];
  showCityDropdown = false;

  states: string[] = [];
  filteredStates: string[] = [];
  showStateDropdown = false;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getProductCategories().subscribe({
      next: (cats) => {
        this.categories = cats;
        this.filteredCategories = cats;
      },
      error: (err) => console.error('Failed to load categories', err)
    });

    this.api.getCustomerCities().subscribe({
      next: (data) => {
        this.cities = data;
        this.filteredCities = data;
      },
      error: (err) => console.error('Failed to load cities', err)
    });

    this.api.getCustomerStates().subscribe({
      next: (data) => {
        this.states = data;
        this.filteredStates = data;
      },
      error: (err) => console.error('Failed to load states', err)
    });
  }

  // Category Dropdown
  onCategoryInput(): void {
    const search = (this.customOrder.productCategory || '').toLowerCase();
    this.filteredCategories = this.categories.filter(c => c.toLowerCase().includes(search));
    this.showDropdown = true;
  }

  selectCategory(category: string): void {
    this.customOrder.productCategory = category;
    this.showDropdown = false;
  }

  hideDropdownDelayed(): void {
    setTimeout(() => { this.showDropdown = false; }, 200);
  }

  // City Dropdown
  onCityInput(): void {
    const search = (this.customOrder.customerCity || '').toLowerCase();
    this.filteredCities = this.cities.filter(c => c.toLowerCase().includes(search));
    this.showCityDropdown = true;
  }

  selectCity(city: string): void {
    this.customOrder.customerCity = city;
    this.showCityDropdown = false;
  }

  hideCityDropdownDelayed(): void {
    setTimeout(() => { this.showCityDropdown = false; }, 200);
  }

  // State Dropdown
  onStateInput(): void {
    const search = (this.customOrder.customerState || '').toLowerCase();
    this.filteredStates = this.states.filter(c => c.toLowerCase().includes(search));
    this.showStateDropdown = true;
  }

  selectState(state: string): void {
    this.customOrder.customerState = state;
    this.showStateDropdown = false;
  }

  hideStateDropdownDelayed(): void {
    setTimeout(() => { this.showStateDropdown = false; }, 200);
  }

  clearForm(): void {
    this.customOrder = {
      productCategory: '',
      productPrice: null,
      freightValue: null,
      productWeightG: null,
      productLengthCm: null,
      productHeightCm: null,
      productWidthCm: null,
      productPhotosQty: null,
      customerCity: '',
      customerState: ''
    };
    this.prediction = null;
    this.errorMessage = '';
  }

  evaluateOrder(): void {
    this.loading = true;
    this.prediction = null;
    this.errorMessage = '';

    this.api.evaluateCustomOrder(this.customOrder).subscribe({
      next: (result) => {
        this.prediction = result;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.error || 'Evaluation failed. Check if ML service is running.';
        this.loading = false;
        console.error(err);
      },
    });
  }

  /** Gauge calculations */
  get gaugeCircumference(): number {
    return 2 * Math.PI * 64; // r = 64
  }

  get gaugeDashoffset(): number {
    if (!this.prediction) return this.gaugeCircumference;
    return this.gaugeCircumference * (1 - this.prediction.probability);
  }

  get gaugeColor(): string {
    if (!this.prediction) return 'var(--text-muted)';
    switch (this.prediction.riskCategory) {
      case 'Green': return 'var(--green)';
      case 'Yellow': return 'var(--yellow)';
      case 'Red': return 'var(--red)';
      default: return 'var(--accent)';
    }
  }

  get riskBadgeClass(): string {
    if (!this.prediction) return '';
    return 'badge-' + this.prediction.riskCategory.toLowerCase();
  }

  get bannerClass(): string {
    if (!this.prediction) return '';
    return 'banner-' + this.prediction.riskCategory.toLowerCase();
  }

  get bannerIcon(): string {
    if (!this.prediction) return '';
    switch (this.prediction.riskCategory) {
      case 'Green': return '✅';
      case 'Yellow': return '⚠️';
      case 'Red': return '🚨';
      default: return 'ℹ️';
    }
  }

  get netExposure(): number {
    if (!this.prediction) return 0;
    return this.prediction.fnCost - this.prediction.fpCost;
  }

  formatCurrency(value: number): string {
    return 'R$ ' + value.toFixed(2);
  }

  formatPercent(value: number): string {
    return (value * 100).toFixed(1) + '%';
  }
}
