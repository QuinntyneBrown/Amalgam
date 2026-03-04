import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ScanService } from './scan.service';
import { RepositoryConfig } from '../models/repository-config';
import { RepositoryType } from '../models/repository-type';

describe('ScanService', () => {
  let service: ScanService;
  let httpTesting: HttpTestingController;

  const mockRepos: RepositoryConfig[] = [
    {
      name: 'discovered-repo',
      type: RepositoryType.Microservice,
      path: '/projects/discovered',
      enabled: true,
    },
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ScanService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should scan a path for repositories', () => {
    service.scan('/projects').subscribe((result) => {
      expect(result).toEqual(mockRepos);
    });
    const req = httpTesting.expectOne('/api/scan');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ path: '/projects' });
    req.flush(mockRepos);
  });
});
