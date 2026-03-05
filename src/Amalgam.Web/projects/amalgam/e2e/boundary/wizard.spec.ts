import { test, expect, Page } from '@playwright/test';
import { WizardPage } from '../pages/wizard.page';

const mockTemplates = [
  { id: 'basic-microservice', name: 'Basic Microservice', description: 'A minimal configuration', repositoryCount: 3 },
  { id: 'full-stack', name: 'Full Stack', description: 'Complete multi-repo setup', repositoryCount: 6 },
];

const mockTemplateInfo = {
  id: 'basic-microservice',
  name: 'Basic Microservice',
  description: 'A minimal configuration',
  repositoryCount: 3,
  config: {
    repositories: [
      { name: 'svc', type: 'Microservice', path: '/svc', enabled: true },
      { name: 'lib', type: 'Library', path: '/lib', enabled: true },
      { name: 'dash', type: 'Dashboard', path: '/dash', enabled: true },
    ],
    backend: { port: 5000, environment: {} },
    frontend: { port: 3000 },
  },
};

async function mockAllApis(page: Page) {
  await page.route('**/api/templates/*', (route) =>
    route.fulfill({ json: mockTemplateInfo })
  );
  await page.route('**/api/templates', (route) =>
    route.fulfill({ json: mockTemplates })
  );
  await page.route('**/api/scan', (route) =>
    route.fulfill({
      json: [
        { name: 'detected-svc', type: 'Microservice', path: '/repos/svc', enabled: true },
      ],
    })
  );
  await page.route('**/api/config**', (route) => {
    if (route.request().method() === 'PUT') {
      route.fulfill({ json: mockTemplateInfo.config });
    } else {
      route.fulfill({
        json: {
          repositories: [],
          backend: { port: 5000, environment: {} },
          frontend: { port: 3000 },
        },
      });
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

test.describe('Wizard Page', () => {
  let wizard: WizardPage;

  test.beforeEach(async ({ page }) => {
    wizard = new WizardPage(page);
    await mockAllApis(page);
  });

  test('wizard renders with step indicator', async () => {
    await wizard.goto();
    await expect(wizard.stepIndicator).toBeVisible();
  });

  test('step 0 shows choice cards', async ({ page }) => {
    await wizard.goto();

    const freshCard = page.locator('ui-card').filter({ hasText: /start fresh/i });
    const templateCard = page.locator('ui-card').filter({ hasText: /use template/i });
    const scanCard = page.locator('ui-card').filter({ hasText: /scan directory/i });

    await expect(freshCard).toBeVisible();
    await expect(templateCard).toBeVisible();
    await expect(scanCard).toBeVisible();
  });

  test('can select fresh and navigate to step 1', async ({ page }) => {
    await wizard.goto();

    // Select "Start Fresh"
    await page.locator('ui-card').filter({ hasText: /start fresh/i }).click();
    await wizard.clickNext();

    // Step 1 should show basic config fields
    const portInput = page.locator('ui-input').filter({ hasText: /backend port/i });
    await expect(portInput).toBeVisible();
  });

  test('back button navigates to previous step', async ({ page }) => {
    await wizard.goto();

    // Select a choice and go to step 1
    await page.locator('ui-card').filter({ hasText: /start fresh/i }).click();
    await wizard.clickNext();

    // Go back
    await wizard.clickBack();

    // Should be on step 0 again with choice cards
    const freshCard = page.locator('ui-card').filter({ hasText: /start fresh/i });
    await expect(freshCard).toBeVisible();
  });

  test('template flow: select template and advance', async ({ page }) => {
    await wizard.goto();

    // Select "Use Template"
    await page.locator('ui-card').filter({ hasText: /use template/i }).click();
    await wizard.clickNext();

    // Step 1 should show template list
    const templateCards = page.locator('ui-card').filter({ hasText: /basic microservice|full stack/i });
    await expect(templateCards.first()).toBeVisible({ timeout: 5000 });
  });

  test('can complete full wizard flow with fresh config', async ({ page }) => {
    await wizard.goto();

    // Step 0: Choose fresh
    await page.locator('ui-card').filter({ hasText: /start fresh/i }).click();
    await wizard.clickNext();

    // Step 1: Configure - just advance (defaults are fine)
    await wizard.clickNext();

    // Step 2: Review - check summary visible
    const summary = page.locator('ui-card').filter({ hasText: /summary/i });
    await expect(summary).toBeVisible();
    await wizard.clickNext();

    // Step 3: Apply - button text changes to "Apply"
    const applyButton = page.locator('ui-button').filter({ hasText: /apply/i }).first();
    await expect(applyButton).toBeVisible();
    await applyButton.click();

    // Should navigate to dashboard
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 5000 });
  });
});
