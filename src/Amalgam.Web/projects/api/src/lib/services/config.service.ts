import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AmalgamConfig } from '../models/amalgam-config';
import { ValidationResult } from '../models/validation-result';
import { API_BASE_URL } from '../tokens';

@Injectable({ providedIn: 'root' })
export class ConfigService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  getConfig(): Observable<AmalgamConfig> {
    return this.http.get<AmalgamConfig>(`${this.baseUrl}/config`);
  }

  updateConfig(config: AmalgamConfig): Observable<AmalgamConfig> {
    return this.http.put<AmalgamConfig>(`${this.baseUrl}/config`, config);
  }

  getYaml(): Observable<string> {
    return this.http.get(`${this.baseUrl}/config/yaml`, { responseType: 'text' });
  }

  updateYaml(yaml: string): Observable<void> {
    const headers = new HttpHeaders({ 'Content-Type': 'text/plain' });
    return this.http.put<void>(`${this.baseUrl}/config/yaml`, yaml, { headers });
  }

  validate(config: AmalgamConfig): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.baseUrl}/config/validate`, config);
  }
}
