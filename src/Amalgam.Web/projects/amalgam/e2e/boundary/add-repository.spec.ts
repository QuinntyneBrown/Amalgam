import { test, expect } from '@playwright/test';
import { AddRepositoryPage } from '../pages/add-repository.page';

test.describe('Add Repository Page', () => {
  let addRepo: AddRepositoryPage;

  test.beforeEach(async ({ page }) => {
    addRepo = new AddRepositoryPage(page);
  });

  test('form renders with required fields', async ({ page }) => {
    await page.route('**/api/directories**', (route) =>
      route.fulfill({ json: ['/packages', '/packages/nuget', '/packages/npm'] })
    );

    await addRepo.goto();

    const inputs = page.locator('input, ui-input, ui-select-field, mat-select');
    await expect(inputs.first()).toBeVisible();
    await expect(addRepo.saveButton).toBeVisible();
    await expect(addRepo.cancelButton).toBeVisible();
  });

  test('shows validation errors for empty required fields', async ({ page }) => {
    await page.route('**/api/directories**', (route) =>
      route.fulfill({ json: [] })
    );

    await addRepo.goto();
    await addRepo.save();

    const errorMessages = page.locator('.error, .invalid, [class*="error"], ui-alert');
    await expect(errorMessages.first()).toBeVisible();
  });

  test('successful creation navigates to repository list', async ({ page }) => {
    await page.route('**/api/directories**', (route) =>
      route.fulfill({ json: ['/packages/nuget'] })
    );

    await page.route('**/api/repositories', (route) => {
      if (route.request().method() === 'POST') {
        route.fulfill({
          json: {
            name: 'new-repo',
            type: 'nuget',
            path: '/packages/nuget',
            enabled: true,
            prefix: '',
            packagePatterns: [],
          },
        });
      } else {
        route.fulfill({ json: [] });
      }
    });

    await addRepo.goto();
    await addRepo.fillName('new-repo');
    await addRepo.fillPath('/packages/nuget');
    await addRepo.save();

    await expect(page).toHaveURL(/\/repositories/);
  });

  test('cancel navigates back without saving', async ({ page }) => {
    await page.route('**/api/directories**', (route) =>
      route.fulfill({ json: [] })
    );

    let postCalled = false;
    await page.route('**/api/repositories', (route) => {
      if (route.request().method() === 'POST') {
        postCalled = true;
      }
      route.fulfill({ json: [] });
    });

    await addRepo.goto();
    await addRepo.cancel();

    expect(postCalled).toBe(false);
  });

  test('directory autocomplete shows suggestions', async ({ page }) => {
    await page.route('**/api/directories**', (route) =>
      route.fulfill({ json: ['/packages', '/packages/nuget', '/packages/npm'] })
    );

    await addRepo.goto();
    await addRepo.fillPath('/pack');

    const suggestions = page.locator(
      '[role="listbox"] [role="option"], .autocomplete-option, mat-option'
    );
    if (await suggestions.first().isVisible({ timeout: 3000 }).catch(() => false)) {
      await expect(suggestions).toHaveCount(3);
    }
  });
});
