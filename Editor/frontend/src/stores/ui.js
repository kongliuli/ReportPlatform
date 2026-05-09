import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useUiStore = defineStore('ui', () => {
  const sidebarCollapsed = ref(false)
  const theme = ref(localStorage.getItem('theme') || 'light')
  const language = ref(localStorage.getItem('language') || 'zh-CN')

  const isDarkMode = computed(() => theme.value === 'dark')

  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  function setTheme(newTheme) {
    theme.value = newTheme
    localStorage.setItem('theme', newTheme)
  }

  function setLanguage(lang) {
    language.value = lang
    localStorage.setItem('language', lang)
  }

  return {
    sidebarCollapsed,
    theme,
    language,
    isDarkMode,
    toggleSidebar,
    setTheme,
    setLanguage
  }
})
