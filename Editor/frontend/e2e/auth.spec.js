import { test, expect } from '@playwright/test'

test.describe('认证流程', () => {
  test('登录页面渲染正常', async ({ page }) => {
    await page.goto('/login')
    await expect(page.locator('input').first()).toBeVisible()
    await expect(page.locator('button:has-text("登")')).toBeVisible()
  })

  test('空用户名密码显示校验错误', async ({ page }) => {
    await page.goto('/login')
    await page.locator('button:has-text("登")').click()
    await expect(page.locator('.el-form-item__error').first()).toBeVisible({ timeout: 5000 })
  })

  test('输入框可正常交互', async ({ page }) => {
    await page.goto('/login')
    await page.locator('input').first().fill('testuser')
    await page.locator('input').nth(1).fill('testpass')
    await expect(page.locator('input').first()).toHaveValue('testuser')
    await expect(page.locator('input').nth(1)).toHaveValue('testpass')
  })

  test('导航守卫 - 未登录访问根路径跳转登录', async ({ page }) => {
    await page.goto('/')
    await page.waitForTimeout(2000)
    expect(page.url()).toContain('login')
  })
})
