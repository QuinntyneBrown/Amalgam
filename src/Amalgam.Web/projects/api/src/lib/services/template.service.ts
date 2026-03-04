import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TemplateInfo } from '../models/template-info';
import { TemplateSummary } from '../models/template-summary';
import { API_BASE_URL } from '../tokens';

@Injectable({ providedIn: 'root' })
export class TemplateService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getAll(): Observable<TemplateSummary[]> {
    return this.http.get<TemplateSummary[]>(`${this.baseUrl}/templates`);
  }

  getById(id: string): Observable<TemplateInfo> {
    return this.http.get<TemplateInfo>(`${this.baseUrl}/templates/${id}`);
  }
}
