import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()

  const isLoggedIn = computed(() => authStore.isLoggedIn)
  const currentUser = computed(() => authStore.user)
  const userRole = computed(() => authStore.userRole)

  function hasPermission(permission) {
    return authStore.hasPermission(permission)
  }

  function requireAuth() {
    if (!authStore.isLoggedIn) {
      router.push({ path: '/login', query: { redirect: router.currentRoute.value.fullPath } })
      return false
    }
    return true
  }

  function requireRole(role) {
    if (!authStore.isLoggedIn) {
      router.push({ path: '/login', query: { redirect: router.currentRoute.value.fullPath } })
      return false
    }
    if (authStore.userRole !== role && authStore.userRole !== 'admin') {
      return false
    }
    return true
  }

  async function login(credentials) {
    const result = await authStore.login(credentials)
    const redirect = router.currentRoute.value.query.redirect || '/'
    router.push(redirect)
    return result
  }

  async function logout() {
    await authStore.logout()
    router.push('/login')
  }

  return {
    isLoggedIn,
    currentUser,
    userRole,
    hasPermission,
    requireAuth,
    requireRole,
    login,
    logout
  }
}
