import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RepositoryConfig } from '../models/repository-config';
import { API_BASE_URL } from '../tokens';

@Injectable({ providedIn: 'root' })
export class ScanService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  scan(path: string): Observable<RepositoryConfig[]> {
    return this.http.post<RepositoryConfig[]>(`${this.baseUrl}/scan`, { path });
  }
}
