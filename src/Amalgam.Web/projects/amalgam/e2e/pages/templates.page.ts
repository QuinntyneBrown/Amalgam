import { BasePage } from './base.page';
import { Page } from '@playwright/test';

export class TemplatesPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async goto() {
    await super.goto('/templates');
    await this.waitForLoad();
  }

  get templateCards() {
    return this.page.locator('ui-card');
  }

  async clickUseTemplate(name: string) {
    const card = this.page.locator('ui-card').filter({ hasText: name });
    await card.locator('ui-button').filter({ hasText: /use/i }).click();
  }
}
