import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const HomeView = () => import('@/views/HomeView.vue')
const LoginView = () => import('@/views/LoginView.vue')
const TemplatesView = () => import('@/views/TemplatesView.vue')
const EditorView = () => import('@/views/EditorView.vue')
const TemplateVersionsView = () => import('@/views/TemplateVersionsView.vue')
const SettingsView = () => import('@/views/SettingsView.vue')

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    name: 'Home',
    component: HomeView,
    meta: { requiresAuth: true }
  },
  {
    path: '/templates',
    name: 'Templates',
    component: TemplatesView,
    meta: { requiresAuth: true }
  },
  {
    path: '/editor/:id?',
    name: 'Editor',
    component: EditorView,
    meta: { requiresAuth: true }
  },
  {
    path: '/templates/:id/versions',
    name: 'TemplateVersions',
    component: TemplateVersionsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/settings',
    name: 'Settings',
    component: SettingsView,
    meta: { requiresAuth: true, requiredRole: 'admin' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth !== false && !authStore.isLoggedIn) {
    next({ path: '/login', query: { redirect: to.fullPath } })
    return
  }

  if (to.meta.requiredRole && authStore.userRole !== to.meta.requiredRole && authStore.userRole !== 'admin') {
    next({ path: '/' })
    return
  }

  if (to.path === '/login' && authStore.isLoggedIn) {
    next({ path: '/' })
    return
  }

  next()
})

export default router
