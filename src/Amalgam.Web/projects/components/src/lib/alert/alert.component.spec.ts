import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { AlertComponent } from './alert.component';

describe('AlertComponent', () => {
  let component: AlertComponent;
  let fixture: ComponentFixture<AlertComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlertComponent],
      providers: [provideAnimationsAsync()],
    }).compileComponents();
    fixture = TestBed.createComponent(AlertComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('message', 'Test alert');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display message', () => {
    expect(fixture.nativeElement.textContent).toContain('Test alert');
  });

  it('should show correct icon for severity', () => {
    fixture.componentRef.setInput('severity', 'error');
    fixture.detectChanges();
    expect(component.icon()).toBe('error');
  });

  it('should show dismiss button when dismissible', () => {
    fixture.componentRef.setInput('dismissible', true);
    fixture.detectChanges();
    const btn = fixture.nativeElement.querySelector('.dismiss-btn');
    expect(btn).toBeTruthy();
  });

  it('should emit dismissed on dismiss click', () => {
    fixture.componentRef.setInput('dismissible', true);
    fixture.detectChanges();
    const spy = vi.fn();
    component.dismissed.subscribe(spy);
    fixture.nativeElement.querySelector('.dismiss-btn').click();
    expect(spy).toHaveBeenCalled();
  });
});
