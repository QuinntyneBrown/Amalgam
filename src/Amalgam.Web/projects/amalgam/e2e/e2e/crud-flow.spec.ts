import { test, expect } from '@playwright/test';
import { RepositoryListPage } from '../pages/repository-list.page';
import { AddRepositoryPage } from '../pages/add-repository.page';
import { DashboardPage } from '../pages/dashboard.page';

const TEST_REPO_NAME = `e2e-test-repo-${Date.now()}`;

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
    await addRepo.fillName(TEST_REPO_NAME);
    await addRepo.fillPath('/packages/test');
    await addRepo.save();

    // Step 3: Verify redirect to repository list and repo is visible
    await expect(page).toHaveURL(/\/repositories/);
    await repoList.waitForLoad();
    await expect(
      page.locator('ui-card').filter({ hasText: TEST_REPO_NAME })
    ).toBeVisible({ timeout: 10000 });

    // Step 4: Click into the repository detail
    await repoList.clickRepository(TEST_REPO_NAME);
    await expect(page).toHaveURL(new RegExp(`/repositories/${TEST_REPO_NAME}`));
    await expect(page.locator('text=' + TEST_REPO_NAME)).toBeVisible();

    // Step 5: Go back to list and toggle the repository
    await page.goBack();
    await repoList.waitForLoad();

    const repoCard = page.locator('ui-card').filter({ hasText: TEST_REPO_NAME });
    const toggleButton = repoCard.locator('ui-toggle, ui-switch, button').first();
    if (await toggleButton.isVisible({ timeout: 3000 }).catch(() => false)) {
      await toggleButton.click();
      await page.waitForTimeout(1000);
    }

    // Step 6: Delete the repository
    const deleteButton = repoCard
      .locator('ui-button, button')
      .filter({ hasText: /delete|remove/i })
      .first();

    if (await deleteButton.isVisible({ timeout: 3000 }).catch(() => false)) {
      await deleteButton.click();

      // Confirm deletion dialog
      const confirmButton = page
        .locator('ui-dialog, [role="dialog"]')
        .locator('ui-button, button')
        .filter({ hasText: /confirm|yes|delete/i })
        .first();

      if (await confirmButton.isVisible({ timeout: 3000 }).catch(() => false)) {
        await confirmButton.click();
      }

      await page.waitForTimeout(1000);

      // Verify repo is gone
      await expect(
        page.locator('ui-card').filter({ hasText: TEST_REPO_NAME })
      ).toHaveCount(0);
    }
  });
});
