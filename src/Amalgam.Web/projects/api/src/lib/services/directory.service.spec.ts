import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { DirectoryService } from './directory.service';

describe('DirectoryService', () => {
  let service: DirectoryService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DirectoryService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should browse directories with path only', () => {
    const dirs = ['/home/user/projects', '/home/user/documents'];
    service.browse('/home/user').subscribe((result) => {
      expect(result).toEqual(dirs);
    });
    const req = httpTesting.expectOne('/api/directories?path=%2Fhome%2Fuser');
    expect(req.request.method).toBe('GET');
    req.flush(dirs);
  });

  it('should browse directories with path and prefix', () => {
    const dirs = ['/home/user/projects'];
    service.browse('/home/user', 'pro').subscribe((result) => {
      expect(result).toEqual(dirs);
    });
    const req = httpTesting.expectOne(
      '/api/directories?path=%2Fhome%2Fuser&prefix=pro'
    );
    expect(req.request.method).toBe('GET');
    req.flush(dirs);
  });
});
