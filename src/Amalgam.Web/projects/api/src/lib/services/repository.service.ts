import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RepositoryConfig } from '../models/repository-config';
import { API_BASE_URL } from '../tokens';

@Injectable({ providedIn: 'root' })
export class RepositoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getAll(): Observable<RepositoryConfig[]> {
    return this.http.get<RepositoryConfig[]>(`${this.baseUrl}/repositories`);
  }

  getByName(name: string): Observable<RepositoryConfig> {
    return this.http.get<RepositoryConfig>(`${this.baseUrl}/repositories/${name}`);
  }

  create(repo: RepositoryConfig): Observable<RepositoryConfig> {
    return this.http.post<RepositoryConfig>(`${this.baseUrl}/repositories`, repo);
  }

  update(name: string, repo: RepositoryConfig): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/repositories/${name}`, repo);
  }

  delete(name: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/repositories/${name}`);
  }

  toggle(name: string): Observable<RepositoryConfig> {
    return this.http.patch<RepositoryConfig>(
      `${this.baseUrl}/repositories/${name}/toggle`,
      null
    );
  }
}
