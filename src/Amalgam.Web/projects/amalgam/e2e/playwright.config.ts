import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  timeout: 60000,
  retries: 0,
  workers: 1,
  use: {
    baseURL: 'http://localhost:4200',
    headless: true,
    viewport: { width: 1280, height: 720 },
  },
  webServer: {
    command: 'cd ../../.. && npx ng serve amalgam --port 4200',
    port: 4200,
    reuseExistingServer: true,
    timeout: 120000,
  },
  projects: [
    {
      name: 'boundary',
      testDir: './boundary',
    },
    {
      name: 'e2e',
      testDir: './e2e',
    },
  ],
});
