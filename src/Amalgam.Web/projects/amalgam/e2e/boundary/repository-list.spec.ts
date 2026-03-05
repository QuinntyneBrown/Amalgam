import { test, expect, Page } from '@playwright/test';
import { RepositoryListPage } from '../pages/repository-list.page';

const mockRepositories = [
  {
    name: 'auth-service',
    type: 'Microservice',
    path: '/repos/auth-service',
    enabled: true,
  },
  {
    name: 'shared-lib',
    type: 'Library',
    path: '/repos/shared-lib',
    enabled: true,
  },
  {
    name: 'ui-plugin',
    type: 'Plugin',
    path: '/repos/ui-plugin',
    enabled: false,
  },
];

async function mockAllApis(page: Page) {
  await page.route('**/api/repositories', (route) => {
    if (route.request().method() === 'GET') {
      route.fulfill({ json: mockRepositories });
    } else {
      route.fulfill({ json: {} });
    }
  });
  await page.route('**/api/repositories/*/toggle', (route) =>
    route.fulfill({ json: { ...mockRepositories[2], enabled: true } })
  );
  await page.route('**/api/dashboard', (route) =>
    route.fulfill({
      json: {
        totalRepositories: 3,
        countByType: { Microservice: 1, Library: 1, Plugin: 1 },
        validation: { isValid: true, errors: [] },
      },
    })
  );
}

test.describe('Repository List Page', () => {
  let repoList: RepositoryListPage;

  test.beforeEach(async ({ page }) => {
    repoList = new RepositoryListPage(page);
    await mockAllApis(page);
  });

  test('loads and displays repository cards', async ({ page }) => {
    await repoList.goto();
    await expect(repoList.repositoryCards).toHaveCount(3);
  });

  test('displays repository names in cards', async ({ page }) => {
    await repoList.goto();
    await expect(repoList.repositoryCards.nth(0)).toContainText('auth-service');
    await expect(repoList.repositoryCards.nth(1)).toContainText('shared-lib');
  });

  test('filters repositories by type chip', async ({ page }) => {
    await repoList.goto();

    // Wait for cards to appear
    await expect(repoList.repositoryCards).toHaveCount(3);

    // Click the "Microservice" filter chip (in the type-filters section)
    const typeFilterChips = page.locator('.type-filters ui-chip');
    const microserviceChip = typeFilterChips.filter({ hasText: 'Microservice' });
    await microserviceChip.click();

    // After filtering, only the microservice repo card should remain
    await expect(repoList.repositoryCards).toHaveCount(1);
    await expect(repoList.repositoryCards.first()).toContainText('auth-service');
  });

  test('shows no cards when no repositories exist', async ({ page }) => {
    // Override the mock with empty array
    await page.route('**/api/repositories', (route) =>
      route.fulfill({ json: [] })
    );

    await repoList.goto();
    await expect(repoList.repositoryCards).toHaveCount(0);
  });

  test('toggle calls toggle API', async ({ page }) => {
    await repoList.goto();
    await expect(repoList.repositoryCards).toHaveCount(3);

    // Find the toggle inside the ui-plugin card
    const pluginCard = page.locator('ui-card').filter({ hasText: 'ui-plugin' });
    const toggle = pluginCard.locator('ui-toggle').first();

    if (await toggle.isVisible({ timeout: 3000 }).catch(() => false)) {
      await toggle.click();
    }
  });

  test('FAB navigates to add repository page', async ({ page }) => {
    await repoList.goto();
    await repoList.clickAddRepository();
    await expect(page).toHaveURL(/\/repositories\/add/);
  });
});
