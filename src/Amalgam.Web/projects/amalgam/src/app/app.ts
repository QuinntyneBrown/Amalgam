import { Component } from '@angular/core';
import { LayoutContainerComponent } from 'domain';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [LayoutContainerComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
