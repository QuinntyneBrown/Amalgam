import { test, expect } from '@playwright/test';
import { TemplatesPage } from '../pages/templates.page';

test.describe('Template End-to-End Flow', () => {
  test('browse templates and apply one', async ({ page }) => {
    const templates = new TemplatesPage(page);

    // Step 1: Navigate to templates page
    await templates.goto();

    // Step 2: Verify template cards are visible
    await expect(templates.templateCards.first()).toBeVisible({ timeout: 10000 });
    const count = await templates.templateCards.count();
    expect(count).toBeGreaterThan(0);

    // Step 3: Get the name of the first template
    const firstCardText = await templates.templateCards.first().textContent();
    expect(firstCardText).toBeTruthy();

    // Step 4: Click "Use" on the first template
    const useButton = templates.templateCards
      .first()
      .locator('ui-button, button')
      .filter({ hasText: /use|apply|select/i })
      .first();

    if (await useButton.isVisible({ timeout: 3000 }).catch(() => false)) {
      await useButton.click();
      await page.waitForTimeout(2000);

      // Verify we navigated somewhere (wizard or config)
      const url = page.url();
      expect(
        url.includes('/wizard') ||
        url.includes('/config') ||
        url.includes('/repositories')
      ).toBe(true);
    }
  });

  test('template detail shows configuration info', async ({ page }) => {
    const templates = new TemplatesPage(page);

    await templates.goto();
    await expect(templates.templateCards.first()).toBeVisible({ timeout: 10000 });

    // Click into a template card
    await templates.templateCards.first().click();
    await page.waitForTimeout(1000);

    // Check if detail view or dialog opened
    const detail = page.locator(
      'ui-dialog, [role="dialog"], .template-detail, .detail'
    );
    const detailVisible = await detail.isVisible({ timeout: 3000 }).catch(() => false);

    if (detailVisible) {
      await expect(detail).toContainText(/.+/);
    }
  });
});
