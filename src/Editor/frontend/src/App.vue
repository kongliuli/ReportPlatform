<template>
  <AppLayout v-if="authStore.isLoggedIn && route.path !== '/login'">
    <router-view />
  </AppLayout>
  <router-view v-else />
</template>

<script setup>
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import AppLayout from '@/components/common/AppLayout.vue'

const route = useRoute()
const authStore = useAuthStore()
</script>

<style>
*, *::before, *::after {
  box-sizing: border-box;
}

html, body, #app {
  margin: 0;
  padding: 0;
  height: 100%;
  width: 100%;
  font-family: var(--font-family-primary);
  background: var(--color-bg-primary);
  color: var(--color-text-primary);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

::selection {
  background: var(--color-primary-bg);
  color: var(--color-primary);
}

:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

.el-button--primary {
  --el-button-bg-color: var(--color-primary);
  --el-button-border-color: var(--color-primary);
  --el-button-hover-bg-color: var(--color-primary-light);
  --el-button-hover-border-color: var(--color-primary-light);
  --el-button-active-bg-color: var(--color-primary-dark);
  --el-button-active-border-color: var(--color-primary-dark);
}

.el-button--primary:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
}

.el-button--primary:active {
  transform: translateY(0);
}

.el-card {
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-sm);
  transition: box-shadow var(--transition-base), transform var(--transition-base);
}

.el-card:hover {
  box-shadow: var(--shadow-md);
}

.el-table {
  --el-table-border-color: var(--color-border-light);
  --el-table-header-bg-color: var(--color-bg-tertiary);
  --el-table-row-hover-bg-color: var(--color-bg-hover);
}

.el-table th.el-table__cell {
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-secondary);
}

.el-input__wrapper {
  border-radius: var(--radius-md);
  box-shadow: none;
  border: 1px solid var(--color-border);
  transition: all var(--transition-fast);
}

.el-input__wrapper:hover {
  border-color: var(--color-text-tertiary);
}

.el-input__wrapper.is-focus {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-bg);
}

.el-select .el-input__wrapper {
  border-radius: var(--radius-md);
}

.el-dialog {
  border-radius: var(--radius-xl);
  overflow: hidden;
}

.el-dialog__header {
  padding: var(--spacing-5) var(--spacing-6);
  border-bottom: 1px solid var(--color-border-light);
  margin-right: 0;
}

.el-dialog__title {
  font-weight: var(--font-weight-semibold);
  font-size: var(--font-size-lg);
  color: var(--color-text-primary);
}

.el-dialog__body {
  padding: var(--spacing-6);
}

.el-dialog__footer {
  padding: var(--spacing-4) var(--spacing-6);
  border-top: 1px solid var(--color-border-light);
}

.el-tag {
  border-radius: var(--radius-full);
  font-weight: var(--font-weight-medium);
}

.el-tag--success {
  background: var(--color-success-bg);
  border-color: transparent;
  color: var(--color-success);
}

.el-tag--info {
  background: var(--color-info-bg);
  border-color: transparent;
  color: var(--color-info);
}

.el-tag--warning {
  background: var(--color-warning-bg);
  border-color: transparent;
  color: var(--color-warning);
}

.el-tag--danger {
  background: var(--color-error-bg);
  border-color: transparent;
  color: var(--color-error);
}

.el-menu {
  border-right: none !important;
}

.el-menu-item {
  border-radius: var(--radius-md);
  margin: 4px 8px;
  transition: all var(--transition-fast);
}

.el-menu-item:hover {
  background: var(--color-bg-hover) !important;
}

.el-menu-item.is-active {
  background: var(--color-primary-bg) !important;
  color: var(--color-primary) !important;
}

.el-tabs__item {
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  transition: color var(--transition-fast);
}

.el-tabs__item.is-active {
  color: var(--color-primary);
  font-weight: var(--font-weight-semibold);
}

.el-tabs__active-bar {
  background: var(--color-primary);
}

.el-pagination {
  --el-pagination-button-bg-color: var(--color-bg-secondary);
  --el-pagination-hover-color: var(--color-primary);
}

.el-pagination button {
  border-radius: var(--radius-md);
}

.el-pagination .el-pager li {
  border-radius: var(--radius-md);
  font-weight: var(--font-weight-medium);
}

.el-pagination .el-pager li.is-active {
  background: var(--color-primary);
  color: var(--color-text-on-primary);
}

.el-form-item__label {
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
}

.el-tooltip__popper {
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
}

.el-message {
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-lg);
  border: none;
}

.el-message--success {
  background: var(--color-success-bg);
  border-left: 4px solid var(--color-success);
}

.el-message--error {
  background: var(--color-error-bg);
  border-left: 4px solid var(--color-error);
}

.el-message--warning {
  background: var(--color-warning-bg);
  border-left: 4px solid var(--color-warning);
}

.el-message--info {
  background: var(--color-info-bg);
  border-left: 4px solid var(--color-info);
}

.el-dropdown-menu {
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border-light);
  box-shadow: var(--shadow-lg);
  padding: var(--spacing-2);
}

.el-dropdown-menu__item {
  border-radius: var(--radius-md);
  margin: 2px 0;
  transition: all var(--transition-fast);
}

.el-dropdown-menu__item:hover {
  background: var(--color-bg-hover);
  color: var(--color-primary);
}

.el-notification {
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-xl);
  border: none;
}

.el-notification__title {
  font-weight: var(--font-weight-semibold);
}

.el-loading-mask {
  background: rgba(248, 250, 251, 0.8);
  backdrop-filter: blur(4px);
}

.el-loading-spinner .circular {
  stroke: var(--color-primary);
}

.el-empty__description {
  color: var(--color-text-tertiary);
}
</style>
