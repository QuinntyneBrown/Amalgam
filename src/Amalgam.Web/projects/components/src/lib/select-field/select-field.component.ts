import { Component, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

export interface SelectOption {
  value: string;
  label: string;
}

@Component({
  selector: 'ui-select-field',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatSelectModule],
  templateUrl: './select-field.component.html',
  styleUrl: './select-field.component.scss',
})
export class SelectFieldComponent {
  label = input.required<string>();
  options = input.required<SelectOption[]>();
  errorMessage = input<string | undefined>(undefined);

  value = model('');
}
