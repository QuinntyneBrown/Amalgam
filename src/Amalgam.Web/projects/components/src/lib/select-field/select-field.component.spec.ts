import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { SelectFieldComponent } from './select-field.component';

describe('SelectFieldComponent', () => {
  let component: SelectFieldComponent;
  let fixture: ComponentFixture<SelectFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectFieldComponent],
      providers: [provideAnimationsAsync()],
    }).compileComponents();
    fixture = TestBed.createComponent(SelectFieldComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('label', 'Test Select');
    fixture.componentRef.setInput('options', [
      { value: 'a', label: 'Option A' },
      { value: 'b', label: 'Option B' },
    ]);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display label', () => {
    const label = fixture.nativeElement.querySelector('mat-label');
    expect(label.textContent).toContain('Test Select');
  });

  it('should support two-way value binding', () => {
    component.value.set('a');
    fixture.detectChanges();
    expect(component.value()).toBe('a');
  });
});
