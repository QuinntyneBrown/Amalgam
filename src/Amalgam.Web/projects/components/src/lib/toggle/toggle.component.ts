import { Component, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

@Component({
  selector: 'ui-toggle',
  standalone: true,
  imports: [FormsModule, MatSlideToggleModule],
  templateUrl: './toggle.component.html',
  styleUrl: './toggle.component.scss',
})
export class ToggleComponent {
  checked = model(false);
}
