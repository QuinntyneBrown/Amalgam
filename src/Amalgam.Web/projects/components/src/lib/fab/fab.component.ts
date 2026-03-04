import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'ui-fab',
  standalone: true,
  imports: [MatButtonModule, MatIconModule],
  templateUrl: './fab.component.html',
  styleUrl: './fab.component.scss',
})
export class FabComponent {
  icon = input('add');
  label = input<string | undefined>(undefined);

  clicked = output<void>();
}
