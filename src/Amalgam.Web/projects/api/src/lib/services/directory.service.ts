import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../tokens';

@Injectable({ providedIn: 'root' })
export class DirectoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  browse(path: string, prefix?: string): Observable<string[]> {
    let params = new HttpParams().set('path', path);
    if (prefix) {
      params = params.set('prefix', prefix);
    }
    return this.http.get<string[]>(`${this.baseUrl}/directories`, { params });
  }
}
