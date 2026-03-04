import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { AddRepositoryPageComponent } from './add-repository-page.component';

describe('AddRepositoryPageComponent', () => {
  let component: AddRepositoryPageComponent;
  let fixture: ComponentFixture<AddRepositoryPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddRepositoryPageComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideAnimationsAsync(),
        provideRouter([]),
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(AddRepositoryPageComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should have type options', () => {
    expect(component.typeOptions.length).toBe(4);
  });

  it('should default saving to false', () => {
    expect(component.saving()).toBe(false);
  });
});
