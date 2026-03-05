import { Component, computed, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'ui-alert',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './alert.component.html',
  styleUrl: './alert.component.scss',
})
export class AlertComponent {
  severity = input<'error' | 'success' | 'warning' | 'info'>('info');
  message = input<string>('');
  title = input<string | undefined>(undefined);
  description = input<string | undefined>(undefined);
  dismissible = input(false);

  dismissed = output<void>();

  icon = computed(() => {
    const icons: Record<string, string> = {
      error: 'error',
      success: 'check_circle',
      warning: 'warning',
      info: 'info',
    };
    return icons[this.severity()];
  });
}
