import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { BottomNavComponent, BottomNavItem } from './bottom-nav.component';

describe('BottomNavComponent', () => {
  let component: BottomNavComponent;
  let fixture: ComponentFixture<BottomNavComponent>;

  const testItems: BottomNavItem[] = [
    { icon: 'home', label: 'Home', route: '/home' },
    { icon: 'search', label: 'Search', route: '/search' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BottomNavComponent],
      providers: [provideAnimationsAsync()],
    }).compileComponents();
    fixture = TestBed.createComponent(BottomNavComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('items', testItems);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render nav items', () => {
    const items = fixture.nativeElement.querySelectorAll('.nav-item');
    expect(items.length).toBe(2);
  });

  it('should highlight active route', () => {
    fixture.componentRef.setInput('activeRoute', '/home');
    fixture.detectChanges();
    const activeItem = fixture.nativeElement.querySelector('.nav-item.active');
    expect(activeItem.textContent).toContain('Home');
  });

  it('should emit navigate on click', () => {
    const spy = vi.fn();
    component.navigate.subscribe(spy);
    fixture.nativeElement.querySelector('.nav-item').click();
    expect(spy).toHaveBeenCalledWith('/home');
  });
});
