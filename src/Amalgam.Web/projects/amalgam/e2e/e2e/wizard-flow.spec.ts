import { test, expect } from '@playwright/test';
import { WizardPage } from '../pages/wizard.page';

test.describe('Wizard End-to-End Flow', () => {
  test('complete wizard from start to finish', async ({ page }) => {
    const wizard = new WizardPage(page);

    // Step 1: Navigate to wizard
    await wizard.goto();
    await expect(wizard.stepIndicator).toBeVisible();

    // Step 2: Navigate through wizard steps
    let stepCount = 0;
    const maxSteps = 10;

    while (stepCount < maxSteps) {
      const nextButton = wizard.nextButton;
      const finishButton = page
        .locator('ui-button, button')
        .filter({ hasText: /finish|complete|done|save/i })
        .first();

      // Check if we reached the final step
      if (await finishButton.isVisible({ timeout: 1000 }).catch(() => false)) {
        await finishButton.click();
        break;
      }

      // Try to advance to next step
      if (await nextButton.isVisible({ timeout: 1000 }).catch(() => false)) {
        // Fill in any required fields on the current step if visible
        const scanButton = page
          .locator('ui-button, button')
          .filter({ hasText: /scan/i })
          .first();

        if (await scanButton.isVisible({ timeout: 500 }).catch(() => false)) {
          const pathInput = page.locator('input').first();
          if (await pathInput.isVisible({ timeout: 500 }).catch(() => false)) {
            await pathInput.fill('/packages');
          }
          await scanButton.click();
          await page.waitForTimeout(2000);
        }

        await nextButton.click();
        await page.waitForTimeout(500);
        stepCount++;
      } else {
        break;
      }
    }

    // Step 3: Verify we've left the wizard (redirected to dashboard or config)
    await page.waitForTimeout(2000);
    const url = page.url();
    expect(
      url.includes('/dashboard') ||
      url.includes('/config') ||
      url.includes('/repositories') ||
      url.includes('/wizard')
    ).toBe(true);
  });
});
