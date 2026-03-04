import { Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'ui-chip',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './chip.component.html',
  styleUrl: './chip.component.scss',
})
export class ChipComponent {
  label = input.required<string>();
  selected = input(false);
  removable = input(false);

  removed = output<void>();
  selectedChange = output<boolean>();

  onChipClick(): void {
    this.selectedChange.emit(!this.selected());
  }

  onRemove(event: Event): void {
    event.stopPropagation();
    this.removed.emit();
  }
}
