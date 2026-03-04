import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ConfigService } from './config.service';
import { AmalgamConfig } from '../models/amalgam-config';
import { ValidationResult } from '../models/validation-result';

describe('ConfigService', () => {
  let service: ConfigService;
  let httpTesting: HttpTestingController;

  const mockConfig: AmalgamConfig = {
    repositories: [],
    backend: { port: 5000, environment: {} },
    frontend: { port: 4200 },
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ConfigService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should get config', () => {
    service.getConfig().subscribe((result) => {
      expect(result).toEqual(mockConfig);
    });
    const req = httpTesting.expectOne('/api/config');
    expect(req.request.method).toBe('GET');
    req.flush(mockConfig);
  });

  it('should update config', () => {
    service.updateConfig(mockConfig).subscribe((result) => {
      expect(result).toEqual(mockConfig);
    });
    const req = httpTesting.expectOne('/api/config');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(mockConfig);
    req.flush(mockConfig);
  });

  it('should get yaml', () => {
    const yaml = 'repositories: []';
    service.getYaml().subscribe((result) => {
      expect(result).toBe(yaml);
    });
    const req = httpTesting.expectOne('/api/config/yaml');
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('text');
    req.flush(yaml);
  });

  it('should update yaml', () => {
    const yaml = 'repositories: []';
    service.updateYaml(yaml).subscribe();
    const req = httpTesting.expectOne('/api/config/yaml');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toBe(yaml);
    expect(req.request.headers.get('Content-Type')).toBe('text/plain');
    req.flush(null);
  });

  it('should validate config', () => {
    const result: ValidationResult = { isValid: true, errors: [] };
    service.validate(mockConfig).subscribe((res) => {
      expect(res).toEqual(result);
    });
    const req = httpTesting.expectOne('/api/config/validate');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockConfig);
    req.flush(result);
  });
});
