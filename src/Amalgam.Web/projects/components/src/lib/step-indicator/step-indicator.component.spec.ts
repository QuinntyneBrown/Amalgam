import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { StepIndicatorComponent, StepItem } from './step-indicator.component';

describe('StepIndicatorComponent', () => {
  let component: StepIndicatorComponent;
  let fixture: ComponentFixture<StepIndicatorComponent>;

  const testSteps: StepItem[] = [
    { label: 'Step 1', status: 'done' },
    { label: 'Step 2', status: 'active' },
    { label: 'Step 3', status: 'pending' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StepIndicatorComponent],
      providers: [provideAnimationsAsync()],
    }).compileComponents();
    fixture = TestBed.createComponent(StepIndicatorComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('steps', testSteps);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render all steps', () => {
    const steps = fixture.nativeElement.querySelectorAll('.step');
    expect(steps.length).toBe(3);
  });

  it('should show check icon for done steps', () => {
    const doneStep = fixture.nativeElement.querySelector('.step-done mat-icon');
    expect(doneStep.textContent.trim()).toBe('check');
  });

  it('should show number for active/pending steps', () => {
    const activeStep = fixture.nativeElement.querySelector('.step-active .step-circle span');
    expect(activeStep.textContent.trim()).toBe('2');
  });

  it('should render connecting lines between steps', () => {
    const lines = fixture.nativeElement.querySelectorAll('.step-line');
    expect(lines.length).toBe(2);
  });
});
