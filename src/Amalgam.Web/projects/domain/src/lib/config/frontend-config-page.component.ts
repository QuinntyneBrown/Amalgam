import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfigService, AmalgamConfig } from 'api';
import { InputComponent, ButtonComponent, CardComponent, SnackbarService } from 'components';

@Component({
  selector: 'dom-frontend-config-page',
  standalone: true,
  imports: [FormsModule, InputComponent, ButtonComponent, CardComponent],
  templateUrl: './frontend-config-page.component.html',
  styleUrl: './frontend-config-page.component.scss',
})
export class FrontendConfigPageComponent implements OnInit {
  private configService = inject(ConfigService);
  private snackbar = inject(SnackbarService);

  loading = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);

  dashboardPath = signal('');
  port = signal('4200');

  private config: AmalgamConfig | null = null;

  ngOnInit(): void {
    this.configService.getConfig().subscribe({
      next: (cfg) => {
        this.config = cfg;
        this.dashboardPath.set(cfg.frontend.dashboardPath ?? '');
        this.port.set(String(cfg.frontend.port));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load configuration');
        this.loading.set(false);
      },
    });
  }

  onSave(): void {
    if (!this.config) return;

    const updated: AmalgamConfig = {
      ...this.config,
      frontend: {
        dashboardPath: this.dashboardPath() || undefined,
        port: Number(this.port()),
      },
    };

    this.saving.set(true);
    this.configService.updateConfig(updated).subscribe({
      next: (cfg) => {
        this.config = cfg;
        this.snackbar.success('Frontend configuration saved');
        this.saving.set(false);
      },
      error: () => {
        this.snackbar.error('Failed to save configuration');
        this.saving.set(false);
      },
    });
  }
}
