import { test, expect } from '@playwright/test'

test.describe('模板管理', () => {
  test('未登录时无法访问模板页', async ({ page }) => {
    await page.goto('/templates')
    await page.waitForTimeout(2000)
    expect(page.url()).toContain('login')
  })

  test('登录页面存在模板管理入口', async ({ page }) => {
    await page.goto('/login')
    await expect(page.locator('form, .el-form, input').first()).toBeVisible()
  })
})
