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
  isTablet = signal(false);
  isDesktop = signal(true);

  navItems: NavItem[] = [
    { icon: 'dashboard', label: 'Dashboard', route: '/dashboard' },
    { icon: 'folder_copy', label: 'Repositories', route: '/repositories' },
    { icon: 'tune', label: 'Backend Config', route: '/config/backend' },
    { icon: 'web', label: 'Frontend Config', route: '/config/frontend' },
    { icon: 'description', label: 'Templates', route: '/templates' },
    { icon: 'code', label: 'YAML Preview', route: '/config/yaml' },
  ];

  ngOnInit(): void {
    this.subscription = this.breakpointObserver
      .observe(['(max-width: 767px)', '(min-width: 768px) and (max-width: 1023px)'])
      .subscribe((result) => {
        const mobile = result.breakpoints['(max-width: 767px)'];
        const tablet = result.breakpoints['(min-width: 768px) and (max-width: 1023px)'];
        this.isMobile.set(mobile);
        this.isTablet.set(tablet);
        this.isDesktop.set(!mobile && !tablet);
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
