import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('accessToken') || '')
  const refreshToken = ref(localStorage.getItem('refreshToken') || '')
  const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))
  const permissions = ref([])

  const isLoggedIn = computed(() => !!token.value)
  const userRole = computed(() => user.value?.role || '')
  const hasPermission = computed(() => {
    return (permission) => permissions.value.includes(permission)
  })

  async function login(credentials) {
    try {
      const { loginApi } = await import('@/api/auth')
      const result = await loginApi(credentials)
      token.value = result.accessToken
      refreshToken.value = result.refreshToken
      user.value = result.user
      localStorage.setItem('accessToken', result.accessToken)
      localStorage.setItem('refreshToken', result.refreshToken)
      localStorage.setItem('user', JSON.stringify(result.user))
      return result
    } catch (error) {
      throw error
    }
  }

  async function refreshAccessToken() {
    try {
      const { refreshTokenApi } = await import('@/api/auth')
      const result = await refreshTokenApi(refreshToken.value)
      token.value = result.accessToken
      localStorage.setItem('accessToken', result.accessToken)
      return result
    } catch (error) {
      logout()
      throw error
    }
  }

  async function logout() {
    try {
      const { logoutApi } = await import('@/api/auth')
      await logoutApi()
    } catch {
    } finally {
      token.value = ''
      refreshToken.value = ''
      user.value = null
      permissions.value = []
      localStorage.removeItem('accessToken')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')
    }
  }

  function checkAuth() {
    return !!token.value
  }

  return {
    token,
    refreshToken,
    user,
    permissions,
    isLoggedIn,
    userRole,
    hasPermission,
    login,
    refreshAccessToken,
    logout,
    checkAuth
  }
})
