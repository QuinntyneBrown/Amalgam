import { BasePage } from './base.page';
import { Page } from '@playwright/test';

export class DashboardPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async goto() {
    await super.goto('/dashboard');
    await this.waitForLoad();
  }

  get totalReposCard() {
    return this.page.locator('ui-card').filter({ hasText: /total|repositories/i }).first();
  }

  get validationAlert() {
    return this.page.locator('ui-alert').first();
  }

  get fab() {
    return this.page.locator('ui-fab').first();
  }

  get typeChips() {
    return this.page.locator('ui-chip');
  }

  async clickFab() {
    await this.fab.click();
  }
}
