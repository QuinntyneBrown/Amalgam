import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { SnackbarService } from './snackbar.service';

describe('SnackbarService', () => {
  let service: SnackbarService;
  let snackBar: MatSnackBar;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideAnimationsAsync()],
    });
    service = TestBed.inject(SnackbarService);
    snackBar = TestBed.inject(MatSnackBar);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should open snackbar with show()', () => {
    const spy = vi.spyOn(snackBar, 'open');
    service.show('Hello', 'OK', { duration: 5000 });
    expect(spy).toHaveBeenCalledWith('Hello', 'OK', {
      duration: 5000,
      panelClass: undefined,
    });
  });

  it('should open success snackbar', () => {
    const spy = vi.spyOn(snackBar, 'open');
    service.success('Done');
    expect(spy).toHaveBeenCalledWith('Done', undefined, {
      duration: 3000,
      panelClass: ['snackbar-success'],
    });
  });

  it('should open error snackbar', () => {
    const spy = vi.spyOn(snackBar, 'open');
    service.error('Failed');
    expect(spy).toHaveBeenCalledWith('Failed', undefined, {
      duration: 3000,
      panelClass: ['snackbar-error'],
    });
  });

  it('should open info snackbar', () => {
    const spy = vi.spyOn(snackBar, 'open');
    service.info('Info');
    expect(spy).toHaveBeenCalledWith('Info', undefined, {
      duration: 3000,
      panelClass: ['snackbar-info'],
    });
  });
});
