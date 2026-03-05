import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DashboardService } from 'api';
import { DashboardSummary } from 'api';
import { AlertComponent, ButtonComponent } from 'components';

@Component({
  selector: 'dom-dashboard-page',
  standalone: true,
  imports: [AlertComponent, ButtonComponent],
  templateUrl: './dashboard-page.component.html',
  styleUrl: './dashboard-page.component.scss',
})
export class DashboardPageComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private router = inject(Router);

  loading = signal(true);
  error = signal<string | null>(null);
  summary = signal<DashboardSummary | null>(null);

  statCards = [
    { type: 'Microservice', label: 'Microservices', bgColor: '#1E3A5F', textColor: '#93C5FD' },
    { type: 'Library', label: 'Libraries', bgColor: '#1A2E1A', textColor: '#86EFAC' },
    { type: 'Plugin', label: 'Plugins', bgColor: '#2E1A3B', textColor: '#C4B5FD' },
    { type: 'Dashboard', label: 'Dashboard', bgColor: '#2E2A1A', textColor: '#FCD34D' },
  ];

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

  getCount(s: DashboardSummary, type: string): number {
    return s.countByType[type] ?? 0;
  }

  onNewConfig(): void {
    this.router.navigate(['/wizard']);
  }

  onRunWizard(): void {
    this.router.navigate(['/wizard']);
  }

  onAddRepo(): void {
    this.router.navigate(['/repositories/add']);
  }

  onValidate(): void {
    this.ngOnInit();
  }

  onExportYaml(): void {
    this.router.navigate(['/config/yaml']);
  }
}
