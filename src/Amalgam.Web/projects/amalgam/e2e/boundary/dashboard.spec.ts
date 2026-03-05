import { test, expect, Page } from '@playwright/test';
import { DashboardPage } from '../pages/dashboard.page';

const mockDashboard = {
  totalRepositories: 5,
  countByType: {
    Microservice: 2,
    Library: 1,
    Plugin: 1,
    Dashboard: 1,
  },
  validation: {
    isValid: true,
    errors: [],
  },
};

const mockDashboardWithErrors = {
  totalRepositories: 3,
  countByType: { Microservice: 2, Library: 1 },
  validation: {
    isValid: false,
    errors: ['Repository "my-repo" has invalid path', 'Duplicate repository name found'],
  },
};

async function mockAllApis(page: Page, dashboardData = mockDashboard) {
  await page.route('**/api/dashboard', (route) =>
    route.fulfill({ json: dashboardData })
  );
  // Mock any other routes that might be hit during navigation
  await page.route('**/api/repositories**', (route) =>
    route.fulfill({ json: [] })
  );
}

test.describe('Dashboard Page', () => {
  let dashboard: DashboardPage;

  test.beforeEach(async ({ page }) => {
    dashboard = new DashboardPage(page);
  });

  test('loads and displays summary stats', async ({ page }) => {
    await mockAllApis(page);
    await dashboard.goto();

    await expect(dashboard.totalReposCard).toBeVisible();
    await expect(dashboard.totalReposCard).toContainText('5');
  });

  test('displays repository type chips', async ({ page }) => {
    await mockAllApis(page);
    await dashboard.goto();

    // The dashboard shows one chip per type entry in countByType
    const chips = dashboard.typeChips;
    await expect(chips.first()).toBeVisible({ timeout: 5000 });
    const count = await chips.count();
    expect(count).toBe(4);
  });

  test('shows validation error alert when config is invalid', async ({ page }) => {
    await mockAllApis(page, mockDashboardWithErrors);
    await dashboard.goto();

    // The template shows an error alert with "Configuration has errors"
    const errorAlert = page.locator('ui-alert').filter({ hasText: /error/i }).first();
    await expect(errorAlert).toBeVisible();
  });

  test('shows success alert when config is valid', async ({ page }) => {
    await mockAllApis(page);
    await dashboard.goto();

    // The template shows a success alert with "Configuration is valid"
    const successAlert = page.locator('ui-alert').filter({ hasText: /valid/i }).first();
    await expect(successAlert).toBeVisible();
  });

  test('FAB navigates to add repository page', async ({ page }) => {
    await mockAllApis(page);
    await dashboard.goto();
    await dashboard.clickFab();

    await expect(page).toHaveURL(/\/repositories\/add/);
  });
});
