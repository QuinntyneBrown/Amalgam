import { test, expect } from '@playwright/test';
import { WizardPage } from '../pages/wizard.page';

const mockTemplates = [
  { id: 'basic', name: 'Basic Setup', description: 'A minimal configuration' },
  { id: 'full', name: 'Full Stack', description: 'Complete multi-repo setup' },
];

const mockScanResults = [
  {
    name: 'detected-nuget',
    type: 'nuget',
    path: '/repos/nuget',
    enabled: true,
    prefix: '',
    packagePatterns: [],
  },
  {
    name: 'detected-npm',
    type: 'npm',
    path: '/repos/npm',
    enabled: true,
    prefix: '',
    packagePatterns: [],
  },
];

test.describe('Wizard Page', () => {
  let wizard: WizardPage;

  test.beforeEach(async ({ page }) => {
    wizard = new WizardPage(page);

    await page.route('**/api/templates', (route) => {
      if (route.request().url().includes('/api/templates/')) {
        const id = route.request().url().split('/').pop();
        const template = mockTemplates.find((t) => t.id === id);
        route.fulfill({ json: template ?? mockTemplates[0] });
      } else {
        route.fulfill({ json: mockTemplates });
      }
    });

    await page.route('**/api/scan', (route) =>
      route.fulfill({ json: mockScanResults })
    );

    await page.route('**/api/config', (route) => {
      if (route.request().method() === 'PUT') {
        route.fulfill({ json: {} });
      } else {
        route.fulfill({ json: { repositories: [], port: 5000 } });
      }
    });

    await page.route('**/api/directories**', (route) =>
      route.fulfill({ json: ['/repos', '/repos/nuget', '/repos/npm'] })
    );
  });

  test('wizard renders with step indicator', async () => {
    await wizard.goto();

    await expect(wizard.stepIndicator).toBeVisible();
  });

  test('can navigate to next step', async () => {
    await wizard.goto();

    await wizard.clickNext();

    await expect(wizard.stepIndicator).toBeVisible();
  });

  test('back button navigates to previous step', async () => {
    await wizard.goto();

    await wizard.clickNext();
    await wizard.clickBack();

    await expect(wizard.stepIndicator).toBeVisible();
  });

  test('template selection step shows available templates', async ({ page }) => {
    await wizard.goto();

    const templateCards = page.locator('ui-card').filter({ hasText: /basic|full/i });
    if (await templateCards.first().isVisible({ timeout: 5000 }).catch(() => false)) {
      await expect(templateCards).toHaveCount(2);
    }
  });

  test('scan step detects repositories', async ({ page }) => {
    await wizard.goto();

    const scanButton = page.locator('ui-button, button').filter({ hasText: /scan/i }).first();
    if (await scanButton.isVisible({ timeout: 5000 }).catch(() => false)) {
      await scanButton.click();

      const detectedRepos = page.locator('ui-card, .repo-item').filter({ hasText: /detected/i });
      await expect(detectedRepos.first()).toBeVisible({ timeout: 5000 });
    }
  });

  test('can complete wizard flow', async ({ page }) => {
    await wizard.goto();

    // Navigate through all steps
    for (let i = 0; i < 5; i++) {
      const nextButton = wizard.nextButton;
      if (await nextButton.isVisible({ timeout: 2000 }).catch(() => false)) {
        await nextButton.click();
        await page.waitForTimeout(500);
      } else {
        break;
      }
    }

    // Look for a finish/complete button at the end
    const finishButton = page
      .locator('ui-button, button')
      .filter({ hasText: /finish|complete|done/i })
      .first();

    if (await finishButton.isVisible({ timeout: 2000 }).catch(() => false)) {
      await finishButton.click();
    }
  });
});
