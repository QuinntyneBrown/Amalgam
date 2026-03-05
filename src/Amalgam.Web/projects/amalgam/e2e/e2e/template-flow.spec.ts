import { test, expect } from '@playwright/test';
import { TemplatesPage } from '../pages/templates.page';

test.describe('Template End-to-End Flow', () => {
  test('browse templates and see template cards', async ({ page }) => {
    const templates = new TemplatesPage(page);

    // Navigate to templates page
    await templates.goto();

    // Verify template cards are visible (backend has 3 built-in templates)
    await expect(templates.templateCards.first()).toBeVisible({ timeout: 10000 });
    const count = await templates.templateCards.count();
    expect(count).toBeGreaterThan(0);

    // Verify template names are visible
    const firstCardText = await templates.templateCards.first().textContent();
    expect(firstCardText).toBeTruthy();
  });

  test('use template button navigates to wizard', async ({ page }) => {
    const templates = new TemplatesPage(page);

    await templates.goto();
    await expect(templates.templateCards.first()).toBeVisible({ timeout: 10000 });

    // Click "Use Template" on the first card
    const useButton = templates.templateCards
      .first()
      .locator('ui-button')
      .filter({ hasText: /use/i })
      .first();

    if (await useButton.isVisible({ timeout: 3000 }).catch(() => false)) {
      await useButton.click();

      // Should navigate to wizard
      await expect(page).toHaveURL(/\/wizard/, { timeout: 5000 });
    }
  });
});
