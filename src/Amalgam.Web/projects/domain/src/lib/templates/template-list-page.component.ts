import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TemplateService, TemplateSummary } from 'api';
import { CardComponent, ButtonComponent } from 'components';

@Component({
  selector: 'dom-template-list-page',
  standalone: true,
  imports: [CardComponent, ButtonComponent],
  templateUrl: './template-list-page.component.html',
  styleUrl: './template-list-page.component.scss',
})
export class TemplateListPageComponent implements OnInit {
  private templateService = inject(TemplateService);
  private router = inject(Router);

  loading = signal(true);
  error = signal<string | null>(null);
  templates = signal<TemplateSummary[]>([]);

  ngOnInit(): void {
    this.templateService.getAll().subscribe({
      next: (data) => {
        this.templates.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load templates');
        this.loading.set(false);
      },
    });
  }

  onUseTemplate(template: TemplateSummary): void {
    this.router.navigate(['/wizard'], { queryParams: { templateId: template.id } });
  }
}
