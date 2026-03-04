import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { DashboardService } from './dashboard.service';
import { DashboardSummary } from '../models/dashboard-summary';

describe('DashboardService', () => {
  let service: DashboardService;
  let httpTesting: HttpTestingController;

  const mockSummary: DashboardSummary = {
    totalRepositories: 3,
    countByType: { Microservice: 2, Library: 1 },
    validation: { isValid: true, errors: [] },
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DashboardService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should get dashboard summary', () => {
    service.getSummary().subscribe((result) => {
      expect(result).toEqual(mockSummary);
    });
    const req = httpTesting.expectOne('/api/dashboard');
    expect(req.request.method).toBe('GET');
    req.flush(mockSummary);
  });
});
