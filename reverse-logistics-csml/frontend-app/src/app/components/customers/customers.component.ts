import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.component.html',
  styleUrls: []
})
export class CustomerComponent implements OnInit {
  items: any[] = [];
  loading = false;
  errorMessage = '';
  page = 1;
  pageSize = 5;
  pageSizeOptions = [5, 10, 25, 50, 100];
  searchQuery = '';
  totalRecords = 0;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.api.getCustomers(this.page, this.pageSize, this.searchQuery).subscribe({
      next: (res) => {
        this.items = res.data;
        this.totalRecords = res.total;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load data';
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    this.page = 1;
    this.loadData();
  }

  get startIndex(): number {
    return this.totalRecords === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  get endIndex(): number {
    return Math.min(this.page * this.pageSize, this.totalRecords);
  }

  get totalPages(): number {
    return Math.ceil(this.totalRecords / this.pageSize);
  }

  onPageSizeChange(event: any): void {
    this.pageSize = +(event.target.value);
    this.page = 1;
    this.loadData();
  }

  firstPage(): void {
    if (this.page !== 1) {
      this.page = 1;
      this.loadData();
    }
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadData();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.loadData();
    }
  }

  lastPage(): void {
    if (this.page !== this.totalPages) {
      this.page = this.totalPages;
      this.loadData();
    }
  }
}
