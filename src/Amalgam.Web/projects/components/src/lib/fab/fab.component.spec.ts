import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { FabComponent } from './fab.component';

describe('FabComponent', () => {
  let component: FabComponent;
  let fixture: ComponentFixture<FabComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FabComponent],
      providers: [provideAnimationsAsync()],
    }).compileComponents();
    fixture = TestBed.createComponent(FabComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default to add icon', () => {
    expect(component.icon()).toBe('add');
  });

  it('should emit clicked on click', () => {
    const spy = vi.fn();
    component.clicked.subscribe(spy);
    fixture.nativeElement.querySelector('button').click();
    expect(spy).toHaveBeenCalled();
  });

  it('should render extended fab when label provided', () => {
    fixture.componentRef.setInput('label', 'Create');
    fixture.detectChanges();
    const button = fixture.nativeElement.querySelector('button');
    expect(button.textContent).toContain('Create');
  });
});
