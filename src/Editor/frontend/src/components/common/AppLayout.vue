<template>
  <el-container class="app-layout">
    <el-aside
      :width="sidebarCollapsed ? '72px' : '260px'"
      class="app-sidebar"
    >
      <div class="sidebar-header">
        <div class="brand" :class="{ collapsed: sidebarCollapsed }">
          <div class="brand-logo">
            <svg viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect width="32" height="32" rx="8" fill="currentColor"/>
              <path d="M8 12h16M8 16h12M8 20h8" stroke="white" stroke-width="2" stroke-linecap="round"/>
            </svg>
          </div>
          <transition name="fade">
            <span v-if="!sidebarCollapsed" class="brand-text">{{ $t('app.brand') }}</span>
          </transition>
        </div>
      </div>

      <nav class="sidebar-nav">
        <el-menu
          :default-active="currentRoute"
          :collapse="sidebarCollapsed"
          router
          class="sidebar-menu"
        >
          <el-menu-item index="/">
            <el-icon><HomeFilled /></el-icon>
            <template #title>{{ $t('pages.home') }}</template>
          </el-menu-item>
          <el-menu-item index="/templates">
            <el-icon><Document /></el-icon>
            <template #title>{{ $t('pages.templates') }}</template>
          </el-menu-item>
          <el-menu-item v-if="isAdmin" index="/settings">
            <el-icon><Setting /></el-icon>
            <template #title>{{ $t('pages.settings') }}</template>
          </el-menu-item>
        </el-menu>
      </nav>

      <div class="sidebar-footer" v-if="!sidebarCollapsed">
        <div class="version-info">
          <span class="version-label">{{ $t('sidebar.version') }}</span>
          <span class="version-number">v2.2.0</span>
        </div>
      </div>
    </el-aside>

    <el-container class="main-container">
      <el-header class="app-header">
        <div class="header-left">
          <button class="collapse-btn" @click="toggleSidebar" :title="sidebarCollapsed ? $t('sidebar.expand') : $t('sidebar.collapse')">
            <el-icon :size="18">
              <Fold v-if="!sidebarCollapsed" />
              <Expand v-else />
            </el-icon>
          </button>
          <div class="breadcrumb">
            <span class="breadcrumb-item">{{ pageTitle }}</span>
          </div>
        </div>

        <div class="header-right">
          <div class="header-actions">
            <el-tooltip :content="$t('home.newTemplate')" placement="bottom">
              <button class="action-btn" @click="$router.push('/editor')">
                <el-icon><Plus /></el-icon>
              </button>
            </el-tooltip>
          </div>

          <div class="header-divider"></div>

          <el-dropdown @command="handleCommand" trigger="click" placement="bottom-end">
            <div class="user-dropdown-trigger">
              <div class="user-avatar">
                {{ userInitial }}
              </div>
              <div class="user-info">
                <span class="user-name">{{ currentUser?.displayName || currentUser?.username || $t('app.brand') }}</span>
                <span class="user-role">{{ roleLabel }}</span>
              </div>
              <el-icon class="dropdown-arrow"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <div class="dropdown-header">
                  <span>{{ currentUser?.email || 'user@example.com' }}</span>
                </div>
                <el-dropdown-item command="profile">
                  <el-icon><User /></el-icon>
                  <span>{{ $t('userMenu.profile') }}</span>
                </el-dropdown-item>
                <el-dropdown-item command="logout" divided>
                  <el-icon><SwitchButton /></el-icon>
                  <span>{{ $t('userMenu.logout') }}</span>
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <el-main class="app-main">
        <slot />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useUiStore } from '@/stores/ui'
import $t from '@/locales/zh-CN'
import {
  HomeFilled,
  Document,
  Setting,
  Fold,
  Expand,
  Plus,
  ArrowDown,
  User,
  SwitchButton
} from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const uiStore = useUiStore()

const currentRoute = computed(() => route.path)
const sidebarCollapsed = computed(() => uiStore.sidebarCollapsed)
const currentUser = computed(() => authStore.user)
const isAdmin = computed(() => authStore.userRole === 'admin')

const userInitial = computed(() => {
  const name = currentUser.value?.displayName || currentUser.value?.username || 'U'
  return name.charAt(0).toUpperCase()
})

const roleLabel = computed(() => {
  const role = currentUser.value?.role
  const labels = {
    admin: $t('roles.admin'),
    editor: $t('roles.editor'),
    viewer: $t('roles.viewer')
  }
  return labels[role] || role || $t('app.brand')
})

const pageTitle = computed(() => {
  const titles = {
    '/': $t('pages.home'),
    '/templates': $t('pages.templates'),
    '/settings': $t('pages.settings')
  }
  if (route.path.startsWith('/editor')) return $t('pages.editor')
  if (route.path.includes('/versions')) return $t('pages.versions')
  return titles[route.path] || $t('app.title')
})

function toggleSidebar() {
  uiStore.toggleSidebar()
}

async function handleCommand(command) {
  if (command === 'logout') {
    await authStore.logout()
    router.push('/login')
  } else if (command === 'profile') {
    router.push('/settings')
  }
}
</script>

<style scoped>
.app-layout {
  height: 100vh;
  background: var(--color-bg-primary);
}

.app-sidebar {
  background: var(--color-bg-secondary);
  border-right: 1px solid var(--color-border-light);
  display: flex;
  flex-direction: column;
  transition: width var(--transition-base);
  overflow: hidden;
}

.sidebar-header {
  height: var(--navbar-height);
  display: flex;
  align-items: center;
  padding: 0 var(--spacing-4);
  border-bottom: 1px solid var(--color-border-light);
}

.brand {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
  overflow: hidden;
}

.brand-logo {
  width: 36px;
  height: 36px;
  color: var(--color-primary);
  flex-shrink: 0;
}

.brand-logo svg {
  width: 100%;
  height: 100%;
}

.brand-text {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  white-space: nowrap;
}

.brand.collapsed {
  justify-content: center;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity var(--transition-fast);
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.sidebar-nav {
  flex: 1;
  padding: var(--spacing-3) 0;
  overflow-y: auto;
}

.sidebar-menu {
  background: transparent;
  border: none;
}

.sidebar-menu :deep(.el-menu-item) {
  height: 44px;
  line-height: 44px;
  margin: 2px var(--spacing-2);
  border-radius: var(--radius-md);
  color: var(--color-text-secondary);
  transition: all var(--transition-fast);
}

.sidebar-menu :deep(.el-menu-item:hover) {
  background: var(--color-bg-hover);
  color: var(--color-text-primary);
}

.sidebar-menu :deep(.el-menu-item.is-active) {
  background: var(--color-primary-bg);
  color: var(--color-primary);
  font-weight: var(--font-weight-medium);
}

.sidebar-menu :deep(.el-menu-item .el-icon) {
  font-size: 18px;
}

.sidebar-footer {
  padding: var(--spacing-4);
  border-top: 1px solid var(--color-border-light);
}

.version-info {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-2) var(--spacing-3);
  background: var(--color-bg-tertiary);
  border-radius: var(--radius-md);
}

.version-label {
  font-size: var(--font-size-xs);
  color: var(--color-text-tertiary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.version-number {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  font-family: var(--font-family-mono);
}

.main-container {
  display: flex;
  flex-direction: column;
  background: var(--color-bg-primary);
}

.app-header {
  height: var(--navbar-height);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--spacing-6);
  background: var(--color-bg-secondary);
  border-bottom: 1px solid var(--color-border-light);
}

.header-left {
  display: flex;
  align-items: center;
  gap: var(--spacing-4);
}

.collapse-btn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.collapse-btn:hover {
  background: var(--color-bg-hover);
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.breadcrumb {
  display: flex;
  align-items: center;
}

.breadcrumb-item {
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.header-right {
  display: flex;
  align-items: center;
  gap: var(--spacing-4);
}

.header-actions {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.action-btn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-primary);
  border: none;
  border-radius: var(--radius-md);
  color: var(--color-text-on-primary);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.action-btn:hover {
  background: var(--color-primary-light);
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
}

.header-divider {
  width: 1px;
  height: 24px;
  background: var(--color-border-light);
}

.user-dropdown-trigger {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
  padding: var(--spacing-2) var(--spacing-3);
  border-radius: var(--radius-lg);
  cursor: pointer;
  transition: background var(--transition-fast);
}

.user-dropdown-trigger:hover {
  background: var(--color-bg-hover);
}

.user-avatar {
  width: 36px;
  height: 36px;
  background: linear-gradient(135deg, var(--color-primary) 0%, var(--color-accent) 100%);
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-on-primary);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
}

.user-info {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 1px;
}

.user-name {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
  line-height: 1.2;
}

.user-role {
  font-size: var(--font-size-xs);
  color: var(--color-text-tertiary);
  line-height: 1.2;
}

.dropdown-arrow {
  color: var(--color-text-tertiary);
  transition: transform var(--transition-fast);
}

.dropdown-header {
  padding: var(--spacing-3) var(--spacing-4);
  border-bottom: 1px solid var(--color-border-light);
  font-size: var(--font-size-sm);
  color: var(--color-text-tertiary);
}

:deep(.el-dropdown-menu__item) {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
  padding: var(--spacing-3) var(--spacing-4);
}

:deep(.el-dropdown-menu__item .el-icon) {
  font-size: 16px;
}

.app-main {
  padding: 0;
  background: var(--color-bg-primary);
  overflow: auto;
}
</style>
