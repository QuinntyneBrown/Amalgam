import { test, expect } from '@playwright/test';
import { DashboardPage } from '../pages/dashboard.page';

const mockDashboard = {
  totalRepositories: 5,
  countByType: {
    nuget: 2,
    npm: 1,
    docker: 1,
    maven: 1,
  },
  validation: {
    isValid: true,
    errors: [],
  },
};

const mockDashboardWithErrors = {
  totalRepositories: 3,
  countByType: { nuget: 2, npm: 1 },
  validation: {
    isValid: false,
    errors: ['Repository "my-repo" has invalid path', 'Duplicate repository name found'],
  },
};

test.describe('Dashboard Page', () => {
  let dashboard: DashboardPage;

  test.beforeEach(async ({ page }) => {
    dashboard = new DashboardPage(page);
  });

  test('loads and displays summary stats', async ({ page }) => {
    await page.route('**/api/dashboard', (route) =>
      route.fulfill({ json: mockDashboard })
    );

    await dashboard.goto();

    await expect(dashboard.totalReposCard).toBeVisible();
    await expect(dashboard.totalReposCard).toContainText('5');
  });

  test('displays repository type chips', async ({ page }) => {
    await page.route('**/api/dashboard', (route) =>
      route.fulfill({ json: mockDashboard })
    );

    await dashboard.goto();

    await expect(dashboard.typeChips).toHaveCount(4);
  });

  test('shows validation errors when config is invalid', async ({ page }) => {
    await page.route('**/api/dashboard', (route) =>
      route.fulfill({ json: mockDashboardWithErrors })
    );

    await dashboard.goto();

    await expect(dashboard.validationAlert).toBeVisible();
    await expect(dashboard.validationAlert).toContainText(/invalid path|duplicate/i);
  });

  test('hides validation alert when config is valid', async ({ page }) => {
    await page.route('**/api/dashboard', (route) =>
      route.fulfill({ json: mockDashboard })
    );

    await dashboard.goto();

    await expect(dashboard.validationAlert).not.toBeVisible();
  });

  test('FAB navigates to add repository page', async ({ page }) => {
    await page.route('**/api/dashboard', (route) =>
      route.fulfill({ json: mockDashboard })
    );

    await dashboard.goto();
    await dashboard.clickFab();

    await expect(page).toHaveURL(/\/repositories\/add/);
  });
});
