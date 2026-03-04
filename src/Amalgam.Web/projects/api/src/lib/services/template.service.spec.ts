import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TemplateService } from './template.service';
import { TemplateSummary } from '../models/template-summary';
import { TemplateInfo } from '../models/template-info';

describe('TemplateService', () => {
  let service: TemplateService;
  let httpTesting: HttpTestingController;

  const mockSummary: TemplateSummary = {
    id: 'default',
    name: 'Default Template',
    description: 'A default configuration',
    repositoryCount: 2,
  };

  const mockInfo: TemplateInfo = {
    ...mockSummary,
    config: {
      repositories: [],
      backend: { port: 5000, environment: {} },
      frontend: { port: 4200 },
    },
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TemplateService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should get all templates', () => {
    service.getAll().subscribe((result) => {
      expect(result).toEqual([mockSummary]);
    });
    const req = httpTesting.expectOne('/api/templates');
    expect(req.request.method).toBe('GET');
    req.flush([mockSummary]);
  });

  it('should get template by id', () => {
    service.getById('default').subscribe((result) => {
      expect(result).toEqual(mockInfo);
    });
    const req = httpTesting.expectOne('/api/templates/default');
    expect(req.request.method).toBe('GET');
    req.flush(mockInfo);
  });
});
