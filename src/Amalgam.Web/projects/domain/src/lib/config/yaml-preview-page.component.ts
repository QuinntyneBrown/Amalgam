import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfigService } from 'api';
import { ButtonComponent, CardComponent, AlertComponent, SnackbarService } from 'components';

@Component({
  selector: 'dom-yaml-preview-page',
  standalone: true,
  imports: [FormsModule, ButtonComponent, CardComponent, AlertComponent],
  templateUrl: './yaml-preview-page.component.html',
  styleUrl: './yaml-preview-page.component.scss',
})
export class YamlPreviewPageComponent implements OnInit {
  private configService = inject(ConfigService);
  private snackbar = inject(SnackbarService);

  loading = signal(true);
  saving = signal(false);
  error = signal<string | null>(null);
  yaml = signal('');
  validationErrors = signal<string[]>([]);
  isValid = signal<boolean | null>(null);

  ngOnInit(): void {
    this.configService.getYaml().subscribe({
      next: (data) => {
        this.yaml.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load YAML');
        this.loading.set(false);
      },
    });
  }

  onSave(): void {
    this.saving.set(true);
    this.configService.updateYaml(this.yaml()).subscribe({
      next: () => {
        this.snackbar.success('YAML saved successfully');
        this.saving.set(false);
      },
      error: () => {
        this.snackbar.error('Failed to save YAML');
        this.saving.set(false);
      },
    });
  }

  onValidate(): void {
    this.configService.getConfig().subscribe({
      next: (config) => {
        this.configService.validate(config).subscribe({
          next: (result) => {
            this.isValid.set(result.isValid);
            this.validationErrors.set(result.errors);
          },
          error: () => {
            this.snackbar.error('Validation failed');
          },
        });
      },
    });
  }

  onDownload(): void {
    const blob = new Blob([this.yaml()], { type: 'text/yaml' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'amalgam-config.yaml';
    a.click();
    URL.revokeObjectURL(url);
  }

  onYamlChange(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    this.yaml.set(textarea.value);
  }
}
