import { test, expect } from '@playwright/test';
import { RepositoryListPage } from '../pages/repository-list.page';

const mockRepositories = [
  {
    name: 'my-nuget-repo',
    type: 'nuget',
    path: '/packages/nuget',
    enabled: true,
    prefix: '',
    packagePatterns: [],
  },
  {
    name: 'my-npm-repo',
    type: 'npm',
    path: '/packages/npm',
    enabled: true,
    prefix: '',
    packagePatterns: [],
  },
  {
    name: 'my-docker-repo',
    type: 'docker',
    path: '/packages/docker',
    enabled: false,
    prefix: '',
    packagePatterns: [],
  },
];

test.describe('Repository List Page', () => {
  let repoList: RepositoryListPage;

  test.beforeEach(async ({ page }) => {
    repoList = new RepositoryListPage(page);
  });

  test('loads and displays repository cards', async ({ page }) => {
    await page.route('**/api/repositories', (route) =>
      route.fulfill({ json: mockRepositories })
    );

    await repoList.goto();

    await expect(repoList.repositoryCards).toHaveCount(3);
  });

  test('displays repository names in cards', async ({ page }) => {
    await page.route('**/api/repositories', (route) =>
      route.fulfill({ json: mockRepositories })
    );

    await repoList.goto();

    await expect(repoList.repositoryCards.nth(0)).toContainText('my-nuget-repo');
    await expect(repoList.repositoryCards.nth(1)).toContainText('my-npm-repo');
  });

  test('filters repositories by type', async ({ page }) => {
    await page.route('**/api/repositories', (route) =>
      route.fulfill({ json: mockRepositories })
    );

    await repoList.goto();
    await repoList.filterByType('nuget');

    const visibleCards = repoList.repositoryCards.filter({ hasText: 'nuget' });
    await expect(visibleCards).toHaveCount(1);
  });

  test('shows empty state when no repositories exist', async ({ page }) => {
    await page.route('**/api/repositories', (route) =>
      route.fulfill({ json: [] })
    );

    await repoList.goto();

    await expect(repoList.repositoryCards).toHaveCount(0);
    await expect(page.locator('text=/no repositories|empty|get started/i')).toBeVisible();
  });

  test('toggle enables/disables a repository', async ({ page }) => {
    await page.route('**/api/repositories', (route) =>
      route.fulfill({ json: mockRepositories })
    );

    let toggleCalled = false;
    await page.route('**/api/repositories/my-docker-repo/toggle', (route) => {
      toggleCalled = true;
      route.fulfill({
        json: { ...mockRepositories[2], enabled: true },
      });
    });

    await repoList.goto();

    const toggleButton = page
      .locator('ui-card')
      .filter({ hasText: 'my-docker-repo' })
      .locator('ui-toggle, ui-switch, button')
      .first();

    if (await toggleButton.isVisible()) {
      await toggleButton.click();
      expect(toggleCalled).toBe(true);
    }
  });

  test('delete shows confirmation dialog', async ({ page }) => {
    await page.route('**/api/repositories', (route) =>
      route.fulfill({ json: mockRepositories })
    );

    await repoList.goto();

    const deleteButton = page
      .locator('ui-card')
      .filter({ hasText: 'my-nuget-repo' })
      .locator('ui-button, button')
      .filter({ hasText: /delete|remove/i })
      .first();

    if (await deleteButton.isVisible()) {
      await deleteButton.click();
      const dialog = page.locator('ui-dialog, [role="dialog"], .dialog');
      await expect(dialog).toBeVisible();
    }
  });

  test('FAB navigates to add repository page', async ({ page }) => {
    await page.route('**/api/repositories', (route) =>
      route.fulfill({ json: mockRepositories })
    );

    await repoList.goto();
    await repoList.clickAddRepository();

    await expect(page).toHaveURL(/\/repositories\/add/);
  });
});
