import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { ModelComparison, PredictionsSummary } from '../../models/interfaces';

@Component({
  selector: 'app-model-benchmark',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './model-benchmark.component.html',
  styleUrl: './model-benchmark.component.css',
})
export class ModelBenchmarkComponent implements OnInit {
  models: ModelComparison[] = [];
  summary: PredictionsSummary | null = null;
  loading = true;
  errorMessage = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.api.getModelComparison().subscribe({
      next: (data) => {
        this.models = data;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load model comparison data.';
        this.loading = false;
        console.error(err);
      },
    });

    this.api.getPredictionsSummary().subscribe({
      next: (data) => {
        this.summary = data;
      },
      error: (err) => {
        console.error('Failed to load predictions summary', err);
      },
    });
  }

  get maxLoss(): number {
    if (!this.models.length) return 1;
    return Math.max(...this.models.map((m) => m.totalFinancialLoss));
  }

  barHeight(loss: number): string {
    const pct = (loss / this.maxLoss) * 100;
    return Math.max(pct, 2) + '%';
  }

  barColor(model: ModelComparison): string {
    if (model.isOptimal) return 'var(--green)';
    return 'var(--accent)';
  }

  formatCurrency(value: number): string {
    return 'R$ ' + value.toLocaleString('en', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  formatPercent(value: number | undefined | null): string {
    if (value === undefined || value === null || isNaN(value)) return 'N/A';
    const pct = value <= 1 ? value * 100 : value;
    return pct.toFixed(2) + '%';
  }
}
