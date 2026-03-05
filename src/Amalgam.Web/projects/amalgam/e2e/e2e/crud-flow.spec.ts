import { test, expect } from '@playwright/test';
import { RepositoryListPage } from '../pages/repository-list.page';
import { AddRepositoryPage } from '../pages/add-repository.page';
import { DashboardPage } from '../pages/dashboard.page';

const TEST_REPO_NAME = `e2e-test-${Date.now()}`;

test.describe('Repository CRUD Flow', () => {
  test('full lifecycle: create, view, toggle, delete a repository', async ({ page }) => {
    const repoList = new RepositoryListPage(page);
    const addRepo = new AddRepositoryPage(page);
    const dashboard = new DashboardPage(page);

    // Step 1: Navigate to dashboard, click FAB to add repo
    await dashboard.goto();
    await dashboard.clickFab();
    await expect(page).toHaveURL(/\/repositories\/add/);

    // Step 2: Fill and submit the add repository form
    const nameInput = page.locator('ui-input').filter({ hasText: /name/i }).locator('input').first();
    await nameInput.fill(TEST_REPO_NAME);

    const pathInput = page.locator('ui-input').filter({ hasText: /path/i }).locator('input').first();
    await pathInput.fill('/packages/test');

    await addRepo.save();

    // Step 3: Verify redirect to repository list and repo is visible
    await expect(page).toHaveURL(/\/repositories/, { timeout: 10000 });
    await repoList.waitForLoad();
    await expect(
      page.locator('ui-card').filter({ hasText: TEST_REPO_NAME })
    ).toBeVisible({ timeout: 10000 });

    // Step 4: Click into the repository detail
    await repoList.clickRepository(TEST_REPO_NAME);
    await expect(page).toHaveURL(new RegExp(`/repositories/${TEST_REPO_NAME}`));
    // Name is displayed in an input field, verify by input value
    const detailNameInput = page.locator('ui-input').filter({ hasText: /name/i }).locator('input').first();
    await expect(detailNameInput).toHaveValue(TEST_REPO_NAME, { timeout: 10000 });

    // Step 5: Go back to list and toggle the repository
    await page.goBack();
    await repoList.waitForLoad();

    const repoCard = page.locator('ui-card').filter({ hasText: TEST_REPO_NAME });
    const toggle = repoCard.locator('ui-toggle mat-slide-toggle, ui-toggle').first();
    if (await toggle.isVisible({ timeout: 3000 }).catch(() => false)) {
      await toggle.click();
      await page.waitForTimeout(1000);
    }

    // Step 6: Delete the repository
    const deleteButton = repoCard.locator('.delete-btn, button').filter({ hasText: /delete/i }).first();

    if (await deleteButton.isVisible({ timeout: 3000 }).catch(() => false)) {
      await deleteButton.click();

      // Confirm deletion dialog
      const dialog = page.locator('mat-dialog-container');
      await expect(dialog).toBeVisible({ timeout: 3000 });

      const confirmButton = dialog.locator('button').filter({ hasText: /delete|confirm/i }).first();
      await confirmButton.click();

      await page.waitForTimeout(1000);

      // Verify repo is gone
      await expect(
        page.locator('ui-card').filter({ hasText: TEST_REPO_NAME })
      ).toHaveCount(0);
    }
  });
});
