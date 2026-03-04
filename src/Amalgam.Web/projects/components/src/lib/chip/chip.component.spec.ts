import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ChipComponent } from './chip.component';

describe('ChipComponent', () => {
  let component: ChipComponent;
  let fixture: ComponentFixture<ChipComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChipComponent],
      providers: [provideAnimationsAsync()],
    }).compileComponents();
    fixture = TestBed.createComponent(ChipComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('label', 'Test Chip');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display label', () => {
    expect(fixture.nativeElement.textContent).toContain('Test Chip');
  });

  it('should toggle selected on click', () => {
    const spy = vi.fn();
    component.selectedChange.subscribe(spy);
    fixture.nativeElement.querySelector('.chip').click();
    expect(spy).toHaveBeenCalledWith(true);
  });

  it('should emit removed when remove button clicked', () => {
    fixture.componentRef.setInput('removable', true);
    fixture.detectChanges();
    const spy = vi.fn();
    component.removed.subscribe(spy);
    fixture.nativeElement.querySelector('.remove-btn').click();
    expect(spy).toHaveBeenCalled();
  });
});
