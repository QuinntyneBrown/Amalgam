import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfigService, AmalgamConfig } from 'api';
import { InputComponent, ButtonComponent, CardComponent, SnackbarService } from 'components';

interface EnvEntry {
  key: string;
  value: string;
}

@Component({
  selector: 'dom-backend-config-page',
  standalone: true,
  imports: [FormsModule, InputComponent, ButtonComponent, CardComponent],
  templateUrl: './backend-config-page.component.html',
  styleUrl: './backend-config-page.component.scss',
})
export class BackendConfigPageComponent implements OnInit {
  private configService = inject(ConfigService);
  private snackbar = inject(SnackbarService);

  loading = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);

  port = signal('5000');
  envEntries = signal<EnvEntry[]>([]);

  private config: AmalgamConfig | null = null;

  ngOnInit(): void {
    this.configService.getConfig().subscribe({
      next: (cfg) => {
        this.config = cfg;
        this.port.set(String(cfg.backend.port));
        const entries = Object.entries(cfg.backend.environment).map(([key, value]) => ({ key, value }));
        this.envEntries.set(entries);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load configuration');
        this.loading.set(false);
      },
    });
  }

  addEnvEntry(): void {
    this.envEntries.update((entries) => [...entries, { key: '', value: '' }]);
  }

  removeEnvEntry(index: number): void {
    this.envEntries.update((entries) => entries.filter((_, i) => i !== index));
  }

  updateEntryKey(index: number, key: string): void {
    this.envEntries.update((entries) =>
      entries.map((e, i) => (i === index ? { ...e, key } : e))
    );
  }

  updateEntryValue(index: number, value: string): void {
    this.envEntries.update((entries) =>
      entries.map((e, i) => (i === index ? { ...e, value } : e))
    );
  }

  onSave(): void {
    if (!this.config) return;

    const environment: Record<string, string> = {};
    for (const entry of this.envEntries()) {
      if (entry.key) {
        environment[entry.key] = entry.value;
      }
    }

    const updated: AmalgamConfig = {
      ...this.config,
      backend: {
        port: Number(this.port()),
        environment,
      },
    };

    this.saving.set(true);
    this.configService.updateConfig(updated).subscribe({
      next: (cfg) => {
        this.config = cfg;
        this.snackbar.success('Backend configuration saved');
        this.saving.set(false);
      },
      error: () => {
        this.snackbar.error('Failed to save configuration');
        this.saving.set(false);
      },
    });
  }
}
