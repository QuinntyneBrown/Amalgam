import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { BottomNavComponent } from 'components';
import { Subscription } from 'rxjs';

interface NavItem {
  icon: string;
  label: string;
  route: string;
}

@Component({
  selector: 'dom-layout',
  standalone: true,
  imports: [RouterOutlet, BottomNavComponent],
  templateUrl: './layout-container.component.html',
  styleUrl: './layout-container.component.scss',
})
export class LayoutContainerComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private breakpointObserver = inject(BreakpointObserver);
  private subscription?: Subscription;

  isMobile = signal(false);

  navItems: NavItem[] = [
    { icon: 'dashboard', label: 'Dashboard', route: '/dashboard' },
    { icon: 'folder_copy', label: 'Repos', route: '/repositories' },
    { icon: 'tune', label: 'Config', route: '/config/backend' },
    { icon: 'description', label: 'Templates', route: '/templates' },
  ];

  ngOnInit(): void {
    this.subscription = this.breakpointObserver
      .observe(['(max-width: 767px)'])
      .subscribe((result) => {
        this.isMobile.set(result.matches);
      });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  onNavigate(route: string): void {
    this.router.navigate([route]);
  }

  get activeRoute(): string {
    return this.router.url;
  }
}
