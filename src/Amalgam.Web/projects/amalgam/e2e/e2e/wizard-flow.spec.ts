import { test, expect } from '@playwright/test';
import { WizardPage } from '../pages/wizard.page';

test.describe('Wizard End-to-End Flow', () => {
  test('complete wizard from start to finish using fresh config', async ({ page }) => {
    const wizard = new WizardPage(page);

    // Navigate to wizard
    await wizard.goto();
    await expect(wizard.stepIndicator).toBeVisible();

    // Step 0: Choose "Start Fresh"
    await page.locator('ui-card').filter({ hasText: /start fresh/i }).click();
    await wizard.clickNext();

    // Step 1: Configure - use defaults, advance
    await wizard.clickNext();

    // Step 2: Review - verify summary visible
    const summary = page.locator('ui-card').filter({ hasText: /summary/i });
    await expect(summary).toBeVisible();
    await wizard.clickNext();

    // Step 3: Apply
    const applyButton = page.locator('ui-button').filter({ hasText: /apply/i }).first();
    await expect(applyButton).toBeVisible();
    await applyButton.click();

    // Should redirect to dashboard
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10000 });
  });
});
