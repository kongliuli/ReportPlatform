import { test, expect } from '@playwright/test'

test.describe('编辑器', () => {
  test('未登录无法访问编辑器', async ({ page }) => {
    await page.goto('/editor')
    await page.waitForTimeout(2000)
    expect(page.url()).toContain('login')
  })

  test('登录页面导航到编辑器', async ({ page }) => {
    await page.goto('/login')
    await expect(page.locator('input').first()).toBeVisible()
  })
})
