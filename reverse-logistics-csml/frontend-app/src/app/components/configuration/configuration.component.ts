import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { CostMatrixConfig } from '../../models/interfaces';

@Component({
  selector: 'app-configuration',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './configuration.component.html',
  styleUrl: './configuration.component.css',
})
export class ConfigurationComponent implements OnInit {
  config: CostMatrixConfig | null = null;
  loading = true;
  saving = false;
  errorMessage = '';
  successMessage = '';

  // Form fields
  profitMargin = 0;
  handlingCost = 0;
  threshold = 0;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadConfig();
  }

  loadConfig(): void {
    this.loading = true;
    this.api.getConfig().subscribe({
      next: (data) => {
        this.config = data;
        this.profitMargin = data.profitMarginPercentage;
        this.handlingCost = data.handlingCostPerOrder;
        this.threshold = data.dynamicThreshold;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load configuration. Is the backend running?';
        this.loading = false;
        console.error(err);
      },
    });
  }

  saveConfig(): void {
    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.api
      .updateConfig({
        profitMarginPercentage: this.profitMargin,
        handlingCostPerOrder: this.handlingCost,
        dynamicThreshold: this.threshold,
      })
      .subscribe({
        next: (data) => {
          this.config = data;
          this.saving = false;
          this.successMessage = 'Configuration saved successfully to the database.';
          setTimeout(() => (this.successMessage = ''), 4000);
        },
        error: (err) => {
          this.errorMessage = 'Failed to save configuration.';
          this.saving = false;
          console.error(err);
        },
      });
  }

  get hasChanges(): boolean {
    if (!this.config) return false;
    return (
      this.profitMargin !== this.config.profitMarginPercentage ||
      this.handlingCost !== this.config.handlingCostPerOrder ||
      this.threshold !== this.config.dynamicThreshold
    );
  }

  resetForm(): void {
    if (this.config) {
      this.profitMargin = this.config.profitMarginPercentage;
      this.handlingCost = this.config.handlingCostPerOrder;
      this.threshold = this.config.dynamicThreshold;
    }
  }
}
