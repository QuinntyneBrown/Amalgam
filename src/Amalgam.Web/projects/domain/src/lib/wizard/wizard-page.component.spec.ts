import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { WizardPageComponent } from './wizard-page.component';

describe('WizardPageComponent', () => {
  let component: WizardPageComponent;
  let fixture: ComponentFixture<WizardPageComponent>;
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WizardPageComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideAnimationsAsync(),
        provideRouter([]),
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(WizardPageComponent);
    component = fixture.componentInstance;
    httpTesting = TestBed.inject(HttpTestingController);
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should start on step 0', () => {
    expect(component.currentStep()).toBe(0);
  });

  it('should have 4 steps', () => {
    expect(component.steps().length).toBe(4);
  });

  it('should select a choice', () => {
    component.selectChoice('fresh');
    expect(component.choice()).toBe('fresh');
  });
});
