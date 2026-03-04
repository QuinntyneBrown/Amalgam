import { BasePage } from './base.page';
import { Page } from '@playwright/test';

export class ConfigPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async gotoBackend() {
    await super.goto('/config/backend');
    await this.waitForLoad();
  }

  async gotoYaml() {
    await super.goto('/config/yaml');
    await this.waitForLoad();
  }

  get portInput() {
    return this.page.locator('input[type="number"], ui-input input').first();
  }

  get saveButton() {
    return this.page.locator('ui-button').filter({ hasText: /save/i }).first();
  }

  get yamlTextarea() {
    return this.page.locator('textarea').first();
  }

  get validateButton() {
    return this.page.locator('ui-button').filter({ hasText: /validate/i }).first();
  }

  get downloadButton() {
    return this.page.locator('ui-button').filter({ hasText: /download/i }).first();
  }

  get alerts() {
    return this.page.locator('ui-alert');
  }
}
