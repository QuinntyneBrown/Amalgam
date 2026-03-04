import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { RepositoryService } from './repository.service';
import { RepositoryConfig } from '../models/repository-config';
import { RepositoryType } from '../models/repository-type';

describe('RepositoryService', () => {
  let service: RepositoryService;
  let httpTesting: HttpTestingController;

  const mockRepo: RepositoryConfig = {
    name: 'test-repo',
    type: RepositoryType.Microservice,
    path: '/path/to/repo',
    enabled: true,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(RepositoryService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should get all repositories', () => {
    service.getAll().subscribe((result) => {
      expect(result).toEqual([mockRepo]);
    });
    const req = httpTesting.expectOne('/api/repositories');
    expect(req.request.method).toBe('GET');
    req.flush([mockRepo]);
  });

  it('should get repository by name', () => {
    service.getByName('test-repo').subscribe((result) => {
      expect(result).toEqual(mockRepo);
    });
    const req = httpTesting.expectOne('/api/repositories/test-repo');
    expect(req.request.method).toBe('GET');
    req.flush(mockRepo);
  });

  it('should create a repository', () => {
    service.create(mockRepo).subscribe((result) => {
      expect(result).toEqual(mockRepo);
    });
    const req = httpTesting.expectOne('/api/repositories');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockRepo);
    req.flush(mockRepo);
  });

  it('should update a repository', () => {
    service.update('test-repo', mockRepo).subscribe();
    const req = httpTesting.expectOne('/api/repositories/test-repo');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(mockRepo);
    req.flush(null);
  });

  it('should delete a repository', () => {
    service.delete('test-repo').subscribe();
    const req = httpTesting.expectOne('/api/repositories/test-repo');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should toggle a repository', () => {
    const toggled = { ...mockRepo, enabled: false };
    service.toggle('test-repo').subscribe((result) => {
      expect(result).toEqual(toggled);
    });
    const req = httpTesting.expectOne('/api/repositories/test-repo/toggle');
    expect(req.request.method).toBe('PATCH');
    req.flush(toggled);
  });
});
