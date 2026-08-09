import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../../services/api.service';
import { PredictionHistoryItem } from '../../models/interfaces';

@Component({
  selector: 'app-prediction-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './prediction-history.component.html',
  styleUrl: './prediction-history.component.css',
})
export class PredictionHistoryComponent implements OnInit {
  predictions: PredictionHistoryItem[] = [];
  loading = true;
  errorMessage = '';

  currentPage = 1;
  pageSize = 20;
  hasMore = true;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadPage();
  }

  loadPage(): void {
    this.loading = true;
    this.api.getPredictionsHistory(this.currentPage, this.pageSize).subscribe({
      next: (data) => {
        this.predictions = data;
        this.hasMore = data.length === this.pageSize;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load prediction history.';
        this.loading = false;
        console.error(err);
      },
    });
  }

  prevPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadPage();
    }
  }

  nextPage(): void {
    if (this.hasMore) {
      this.currentPage++;
      this.loadPage();
    }
  }

  badgeClass(risk: string): string {
    return 'badge-' + risk.toLowerCase();
  }

  formatCurrency(value: number): string {
    return 'R$ ' + value.toFixed(2);
  }
}
