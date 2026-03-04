import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RepositoryService, RepositoryType, RepositoryConfig } from 'api';
import {
  InputComponent,
  SelectFieldComponent,
  ButtonComponent,
  CardComponent,
  SnackbarService,
} from 'components';

@Component({
  selector: 'dom-add-repository-page',
  standalone: true,
  imports: [FormsModule, InputComponent, SelectFieldComponent, ButtonComponent, CardComponent],
  templateUrl: './add-repository-page.component.html',
  styleUrl: './add-repository-page.component.scss',
})
export class AddRepositoryPageComponent {
  private repoService = inject(RepositoryService);
  private router = inject(Router);
  private snackbar = inject(SnackbarService);

  name = signal('');
  type = signal('Microservice');
  path = signal('');
  routePrefix = signal('');
  packageName = signal('');
  mergeSources = signal('');
  mergeTarget = signal('');
  saving = signal(false);

  typeOptions = Object.values(RepositoryType).map((t) => ({ value: t, label: t }));

  onSave(): void {
    const repo: Partial<RepositoryConfig> = {
      name: this.name(),
      type: this.type() as RepositoryType,
      path: this.path(),
      enabled: true,
    };

    if (this.routePrefix()) {
      repo.routePrefix = this.routePrefix();
    }
    if (this.packageName()) {
      repo.packageName = this.packageName();
    }
    if (this.mergeSources()) {
      repo.merge = {
        sources: this.mergeSources().split(',').map((s) => s.trim()),
        target: this.mergeTarget() || undefined,
      };
    }

    this.saving.set(true);
    this.repoService.create(repo as RepositoryConfig).subscribe({
      next: () => {
        this.snackbar.success('Repository created successfully');
        this.router.navigate(['/repositories']);
      },
      error: () => {
        this.snackbar.error('Failed to create repository');
        this.saving.set(false);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/repositories']);
  }
}
