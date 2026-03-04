import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DashboardService } from 'api';
import { DashboardSummary } from 'api';
import { CardComponent, ChipComponent, AlertComponent, FabComponent } from 'components';

@Component({
  selector: 'dom-dashboard-page',
  standalone: true,
  imports: [CardComponent, ChipComponent, AlertComponent, FabComponent],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
})
export class DashboardPageComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private router = inject(Router);

  loading = signal(true);
  error = signal<string | null>(null);
  summary = signal<DashboardSummary | null>(null);

  ngOnInit(): void {
    this.dashboardService.getSummary().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load dashboard summary');
        this.loading.set(false);
      },
    });
  }

  get typeEntries(): { type: string; count: number }[] {
    const s = this.summary();
    if (!s) return [];
    return Object.entries(s.countByType).map(([type, count]) => ({ type, count }));
  }

  onAddRepo(): void {
    this.router.navigate(['/repositories/add']);
  }
}
