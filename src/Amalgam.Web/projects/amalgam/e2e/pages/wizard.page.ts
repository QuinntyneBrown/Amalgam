import { BasePage } from './base.page';
import { Page } from '@playwright/test';

export class WizardPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async goto() {
    await super.goto('/wizard');
    await this.waitForLoad();
  }

  get stepIndicator() {
    return this.page.locator('ui-step-indicator').first();
  }

  get nextButton() {
    return this.page.locator('ui-button').filter({ hasText: /next|continue/i }).first();
  }

  get backButton() {
    return this.page.locator('ui-button').filter({ hasText: /back|previous/i }).first();
  }

  async chooseOption(option: string) {
    await this.page.locator('ui-card, ui-button, button').filter({ hasText: new RegExp(option, 'i') }).first().click();
  }

  async clickNext() {
    await this.nextButton.click();
  }

  async clickBack() {
    await this.backButton.click();
  }
}
