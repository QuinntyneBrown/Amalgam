import { Page } from '@playwright/test';

export class BasePage {
  constructor(protected page: Page) {}

  async goto(path: string) {
    await this.page.goto(path);
  }

  async waitForLoad() {
    await this.page.waitForLoadState('networkidle');
  }

  async getTitle() {
    return this.page.title();
  }

  async getNavItems() {
    return this.page.locator('ui-bottom-nav .nav-item, .sidebar .nav-item').all();
  }

  async navigateTo(route: string) {
    const navItem = this.page.locator(`[data-route="${route}"], .nav-item`).filter({ hasText: route });
    if (await navItem.count() > 0) {
      await navItem.first().click();
    } else {
      await this.page.goto(route);
    }
  }
}
