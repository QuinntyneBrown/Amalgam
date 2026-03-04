import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { RepositoryService, RepositoryConfig, RepositoryType } from 'api';
import {
  InputComponent,
  SelectFieldComponent,
  ButtonComponent,
  CardComponent,
  ToggleComponent,
  ConfirmDialogComponent,
  SnackbarService,
} from 'components';

@Component({
  selector: 'dom-repository-detail-page',
  standalone: true,
  imports: [FormsModule, InputComponent, SelectFieldComponent, ButtonComponent, CardComponent, ToggleComponent],
  templateUrl: './repository-detail-page.component.html',
  styleUrl: './repository-detail-page.component.scss',
})
export class RepositoryDetailPageComponent implements OnInit {
  private repoService = inject(RepositoryService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private dialog = inject(MatDialog);
  private snackbar = inject(SnackbarService);

  loading = signal(true);
  error = signal<string | null>(null);
  saving = signal(false);

  name = signal('');
  type = signal('Microservice');
  path = signal('');
  enabled = signal(true);
  routePrefix = signal('');
  packageName = signal('');
  mergeSources = signal('');
  mergeTarget = signal('');

  private originalName = '';

  typeOptions = Object.values(RepositoryType).map((t) => ({ value: t, label: t }));

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.originalName = params['name'];
      this.loadRepo();
    });
  }

  private loadRepo(): void {
    this.repoService.getByName(this.originalName).subscribe({
      next: (repo) => {
        this.name.set(repo.name);
        this.type.set(repo.type);
        this.path.set(repo.path);
        this.enabled.set(repo.enabled);
        this.routePrefix.set(repo.routePrefix ?? '');
        this.packageName.set(repo.packageName ?? '');
        this.mergeSources.set(repo.merge?.sources.join(', ') ?? '');
        this.mergeTarget.set(repo.merge?.target ?? '');
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load repository');
        this.loading.set(false);
      },
    });
  }

  onSave(): void {
    const repo: Partial<RepositoryConfig> = {
      name: this.name(),
      type: this.type() as RepositoryType,
      path: this.path(),
      enabled: this.enabled(),
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
    this.repoService.update(this.originalName, repo as RepositoryConfig).subscribe({
      next: () => {
        this.snackbar.success('Repository updated successfully');
        this.saving.set(false);
      },
      error: () => {
        this.snackbar.error('Failed to update repository');
        this.saving.set(false);
      },
    });
  }

  onToggle(): void {
    this.repoService.toggle(this.originalName).subscribe({
      next: (updated) => {
        this.enabled.set(updated.enabled);
        this.snackbar.success(`Repository ${updated.enabled ? 'enabled' : 'disabled'}`);
      },
    });
  }

  onDelete(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Repository',
        message: `Are you sure you want to delete "${this.name()}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.repoService.delete(this.originalName).subscribe({
          next: () => {
            this.snackbar.success('Repository deleted');
            this.router.navigate(['/repositories']);
          },
        });
      }
    });
  }

  onBack(): void {
    this.router.navigate(['/repositories']);
  }
}
