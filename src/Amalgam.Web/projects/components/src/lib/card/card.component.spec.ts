import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { CardComponent } from './card.component';

describe('CardComponent', () => {
  let component: CardComponent;
  let fixture: ComponentFixture<CardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardComponent],
      providers: [provideAnimationsAsync()],
    }).compileComponents();
    fixture = TestBed.createComponent(CardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display title when provided', () => {
    fixture.componentRef.setInput('title', 'Card Title');
    fixture.detectChanges();
    const title = fixture.nativeElement.querySelector('mat-card-title');
    expect(title.textContent).toContain('Card Title');
  });

  it('should display subtitle when provided', () => {
    fixture.componentRef.setInput('subtitle', 'Card Subtitle');
    fixture.detectChanges();
    const subtitle = fixture.nativeElement.querySelector('mat-card-subtitle');
    expect(subtitle.textContent).toContain('Card Subtitle');
  });
});
