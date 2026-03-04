import { BasePage } from './base.page';
import { Page } from '@playwright/test';

export class RepositoryListPage extends BasePage {
  constructor(page: Page) {
    super(page);
  }

  async goto() {
    await super.goto('/repositories');
    await this.waitForLoad();
  }

  get repositoryCards() {
    return this.page.locator('ui-card');
  }

  get filterChips() {
    return this.page.locator('ui-chip');
  }

  get searchInput() {
    return this.page.locator('ui-input input, input[type="text"]').first();
  }

  get fab() {
    return this.page.locator('ui-fab').first();
  }

  async clickRepository(name: string) {
    await this.page.locator('ui-card').filter({ hasText: name }).click();
  }

  async filterByType(type: string) {
    await this.page.locator('ui-chip').filter({ hasText: type }).click();
  }

  async clickAddRepository() {
    await this.fab.click();
  }
}
