import { Component, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  ConfigService,
  ScanService,
  TemplateService,
  AmalgamConfig,
  RepositoryConfig,
  TemplateSummary,
  TemplateInfo,
} from 'api';
import {
  ButtonComponent,
  CardComponent,
  InputComponent,
  StepIndicatorComponent,
  AlertComponent,
  SnackbarService,
} from 'components';

type WizardChoice = 'fresh' | 'template' | 'scan';
type StepStatus = 'done' | 'active' | 'pending';

@Component({
  selector: 'dom-wizard-page',
  standalone: true,
  imports: [FormsModule, ButtonComponent, CardComponent, InputComponent, StepIndicatorComponent, AlertComponent],
  templateUrl: './wizard-page.component.html',
  styleUrl: './wizard-page.component.scss',
})
export class WizardPageComponent {
  private configService = inject(ConfigService);
  private scanService = inject(ScanService);
  private templateService = inject(TemplateService);
  private router = inject(Router);
  private snackbar = inject(SnackbarService);

  currentStep = signal(0);
  choice = signal<WizardChoice | null>(null);
  scanPath = signal('');
  scannedRepos = signal<RepositoryConfig[]>([]);
  templates = signal<TemplateSummary[]>([]);
  selectedTemplate = signal<TemplateInfo | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  // Fresh config signals
  backendPort = signal('5000');
  frontendPort = signal('4200');
  dashboardPath = signal('');

  steps = computed(() => {
    const step = this.currentStep();
    const labels = ['Choose', 'Configure', 'Review', 'Apply'];
    return labels.map((label, i) => ({
      label,
      status: (i < step ? 'done' : i === step ? 'active' : 'pending') as StepStatus,
    }));
  });

  get configSummary(): AmalgamConfig {
    const template = this.selectedTemplate();
    if (template) {
      return template.config;
    }
    return {
      repositories: this.scannedRepos(),
      backend: { port: Number(this.backendPort()), environment: {} },
      frontend: { port: Number(this.frontendPort()), dashboardPath: this.dashboardPath() || undefined },
    };
  }

  selectChoice(c: WizardChoice): void {
    this.choice.set(c);
  }

  onNext(): void {
    const step = this.currentStep();

    if (step === 0 && !this.choice()) return;

    if (step === 1) {
      if (this.choice() === 'template' && !this.selectedTemplate()) return;
    }

    if (step === 0 && this.choice() === 'template') {
      this.loading.set(true);
      this.templateService.getAll().subscribe({
        next: (t) => {
          this.templates.set(t);
          this.loading.set(false);
          this.currentStep.set(step + 1);
        },
        error: () => {
          this.error.set('Failed to load templates');
          this.loading.set(false);
        },
      });
      return;
    }

    if (step === 3) {
      this.applyConfig();
      return;
    }

    this.currentStep.set(step + 1);
  }

  onBack(): void {
    const step = this.currentStep();
    if (step > 0) {
      this.currentStep.set(step - 1);
    }
  }

  onSelectTemplate(template: TemplateSummary): void {
    this.loading.set(true);
    this.templateService.getById(template.id).subscribe({
      next: (info) => {
        this.selectedTemplate.set(info);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load template');
        this.loading.set(false);
      },
    });
  }

  onScan(): void {
    if (!this.scanPath()) return;
    this.loading.set(true);
    this.scanService.scan(this.scanPath()).subscribe({
      next: (repos) => {
        this.scannedRepos.set(repos);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Scan failed');
        this.loading.set(false);
      },
    });
  }

  private applyConfig(): void {
    this.loading.set(true);
    this.configService.updateConfig(this.configSummary).subscribe({
      next: () => {
        this.snackbar.success('Configuration applied successfully');
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.snackbar.error('Failed to apply configuration');
        this.loading.set(false);
      },
    });
  }
}
