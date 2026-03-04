import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('domain').then((m) => m.DashboardPageComponent),
  },
  {
    path: 'repositories',
    loadComponent: () =>
      import('domain').then((m) => m.RepositoryListPageComponent),
  },
  {
    path: 'repositories/add',
    loadComponent: () =>
      import('domain').then((m) => m.AddRepositoryPageComponent),
  },
  {
    path: 'repositories/:name',
    loadComponent: () =>
      import('domain').then((m) => m.RepositoryDetailPageComponent),
  },
  {
    path: 'config/backend',
    loadComponent: () =>
      import('domain').then((m) => m.BackendConfigPageComponent),
  },
  {
    path: 'config/frontend',
    loadComponent: () =>
      import('domain').then((m) => m.FrontendConfigPageComponent),
  },
  {
    path: 'config/yaml',
    loadComponent: () =>
      import('domain').then((m) => m.YamlPreviewPageComponent),
  },
  {
    path: 'wizard',
    loadComponent: () =>
      import('domain').then((m) => m.WizardPageComponent),
  },
  {
    path: 'templates',
    loadComponent: () =>
      import('domain').then((m) => m.TemplateListPageComponent),
  },
  { path: '**', redirectTo: 'dashboard' },
];
