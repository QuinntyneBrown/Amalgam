import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

export interface BottomNavItem {
  icon: string;
  label: string;
  route: string;
}

@Component({
  selector: 'ui-bottom-nav',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './bottom-nav.component.html',
  styleUrl: './bottom-nav.component.scss',
})
export class BottomNavComponent {
  items = input.required<BottomNavItem[]>();
  activeRoute = input('');

  navigate = output<string>();
}
