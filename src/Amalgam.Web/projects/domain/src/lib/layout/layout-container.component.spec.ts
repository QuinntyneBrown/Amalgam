import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter, Router } from '@angular/router';
import { vi } from 'vitest';
import { LayoutContainerComponent } from './layout-container.component';

describe('LayoutContainerComponent', () => {
  let component: LayoutContainerComponent;
  let fixture: ComponentFixture<LayoutContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LayoutContainerComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideAnimationsAsync(),
        provideRouter([]),
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(LayoutContainerComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should have nav items', () => {
    expect(component.navItems.length).toBe(4);
  });

  it('should navigate on onNavigate', () => {
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockReturnValue(Promise.resolve(true));
    component.onNavigate('/dashboard');
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });
});
