import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { RepositoryService, RepositoryConfig, RepositoryType } from 'api';
import {
  CardComponent,
  ChipComponent,
  InputComponent,
  ToggleComponent,
  FabComponent,
  ConfirmDialogComponent,
} from 'components';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'dom-repository-list-page',
  standalone: true,
  imports: [CardComponent, ChipComponent, InputComponent, ToggleComponent, FabComponent, FormsModule],
  templateUrl: './repository-list-page.component.html',
  styleUrl: './repository-list-page.component.scss',
})
export class RepositoryListPageComponent implements OnInit {
  private repoService = inject(RepositoryService);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  loading = signal(true);
  error = signal<string | null>(null);
  repos = signal<RepositoryConfig[]>([]);
  searchQuery = signal('');
  selectedTypes = signal<Set<RepositoryType>>(new Set());

  allTypes = Object.values(RepositoryType);

  filteredRepos = computed(() => {
    let result = this.repos();
    const query = this.searchQuery().toLowerCase();
    const types = this.selectedTypes();

    if (query) {
      result = result.filter((r) => r.name.toLowerCase().includes(query));
    }
    if (types.size > 0) {
      result = result.filter((r) => types.has(r.type));
    }
    return result;
  });

  ngOnInit(): void {
    this.loadRepos();
  }

  private loadRepos(): void {
    this.loading.set(true);
    this.repoService.getAll().subscribe({
      next: (data) => {
        this.repos.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load repositories');
        this.loading.set(false);
      },
    });
  }

  toggleTypeFilter(type: RepositoryType): void {
    const current = new Set(this.selectedTypes());
    if (current.has(type)) {
      current.delete(type);
    } else {
      current.add(type);
    }
    this.selectedTypes.set(current);
  }

  isTypeSelected(type: RepositoryType): boolean {
    return this.selectedTypes().has(type);
  }

  onSearchChange(value: string): void {
    this.searchQuery.set(value);
  }

  onToggleEnabled(repo: RepositoryConfig): void {
    this.repoService.toggle(repo.name).subscribe({
      next: (updated) => {
        this.repos.update((list) =>
          list.map((r) => (r.name === repo.name ? updated : r))
        );
      },
    });
  }

  onDelete(repo: RepositoryConfig): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Repository',
        message: `Are you sure you want to delete "${repo.name}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.repoService.delete(repo.name).subscribe({
          next: () => {
            this.repos.update((list) => list.filter((r) => r.name !== repo.name));
          },
        });
      }
    });
  }

  onCardClick(repo: RepositoryConfig): void {
    this.router.navigate(['/repositories', repo.name]);
  }

  onAdd(): void {
    this.router.navigate(['/repositories/add']);
  }
}
