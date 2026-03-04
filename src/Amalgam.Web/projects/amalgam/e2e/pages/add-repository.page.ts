import { BasePage } from './base.page';
import { Page } from '@playwright/test';

export class AddRepositoryPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async goto() {
    await super.goto('/repositories/add');
    await this.waitForLoad();
  }

  async fillName(name: string) {
    await this.page.locator('input').filter({ hasNotText: /path|prefix|package/i }).first().fill(name);
  }

  async selectType(type: string) {
    await this.page.locator('mat-select, ui-select-field').first().click();
    await this.page.locator('mat-option').filter({ hasText: type }).click();
  }

  async fillPath(path: string) {
    const pathInputs = this.page.locator('input');
    await pathInputs.nth(1).fill(path);
  }

  get saveButton() {
    return this.page.locator('ui-button').filter({ hasText: /save|create|add/i }).first();
  }

  get cancelButton() {
    return this.page.locator('ui-button').filter({ hasText: /cancel/i }).first();
  }

  async save() {
    await this.saveButton.click();
  }

  async cancel() {
    await this.cancelButton.click();
  }
}
