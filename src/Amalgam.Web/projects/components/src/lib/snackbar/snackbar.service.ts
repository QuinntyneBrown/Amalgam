import { Injectable, inject } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig, MatSnackBarRef, TextOnlySnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class SnackbarService {
  private snackBar = inject(MatSnackBar);

  show(
    message: string,
    action?: string,
    config?: { duration?: number; panelClass?: string }
  ): MatSnackBarRef<TextOnlySnackBar> {
    const snackBarConfig: MatSnackBarConfig = {
      duration: config?.duration ?? 3000,
      panelClass: config?.panelClass ? [config.panelClass] : undefined,
    };
    return this.snackBar.open(message, action, snackBarConfig);
  }

  success(message: string): MatSnackBarRef<TextOnlySnackBar> {
    return this.show(message, undefined, { panelClass: 'snackbar-success' });
  }

  error(message: string): MatSnackBarRef<TextOnlySnackBar> {
    return this.show(message, undefined, { panelClass: 'snackbar-error' });
  }

  info(message: string): MatSnackBarRef<TextOnlySnackBar> {
    return this.show(message, undefined, { panelClass: 'snackbar-info' });
  }
}
