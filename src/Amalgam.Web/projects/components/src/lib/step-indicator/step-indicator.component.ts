import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

export interface StepItem {
  label: string;
  status: 'done' | 'active' | 'pending';
}

@Component({
  selector: 'ui-step-indicator',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './step-indicator.component.html',
  styleUrl: './step-indicator.component.scss',
})
export class StepIndicatorComponent {
  steps = input.required<StepItem[]>();
}
