import { test, expect, Page } from '@playwright/test';
import { AddRepositoryPage } from '../pages/add-repository.page';

async function mockAllApis(page: Page) {
  // Mock all API routes that might be hit during navigation
  await page.route('**/api/repositories**', (route) => {
    if (route.request().method() === 'POST') {
      route.fulfill({
        json: {
          name: 'new-repo',
          type: 'Microservice',
          path: '/packages/test',
          enabled: true,
        },
      });
    } else {
      route.fulfill({ json: [] });
    }
  });
  await page.route('**/api/dashboard', (route) =>
    route.fulfill({
      json: {
        totalRepositories: 0,
        countByType: {},
        validation: { isValid: true, errors: [] },
      },
    })
  );
}

test.describe('Add Repository Page', () => {
  let addRepo: AddRepositoryPage;

  test.beforeEach(async ({ page }) => {
    addRepo = new AddRepositoryPage(page);
    await mockAllApis(page);
  });

  test('form renders with required fields', async ({ page }) => {
    await addRepo.goto();

    // Verify form inputs are visible
    const nameInput = page.locator('ui-input').filter({ hasText: /name/i }).first();
    await expect(nameInput).toBeVisible();
    await expect(addRepo.saveButton).toBeVisible();
    await expect(addRepo.cancelButton).toBeVisible();
  });

  test('cancel navigates to repositories list', async ({ page }) => {
    await addRepo.goto();
    await addRepo.cancel();

    await expect(page).toHaveURL(/\/repositories/, { timeout: 5000 });
  });

  test('successful creation navigates to repository list', async ({ page }) => {
    await addRepo.goto();

    // Fill name
    const nameInput = page.locator('ui-input').filter({ hasText: /name/i }).locator('input').first();
    await nameInput.fill('new-repo');

    // Fill path
    const pathInput = page.locator('ui-input').filter({ hasText: /^path/i }).locator('input').first();
    await pathInput.fill('/packages/test');

    await addRepo.save();

    await expect(page).toHaveURL(/\/repositories/, { timeout: 5000 });
  });
});
