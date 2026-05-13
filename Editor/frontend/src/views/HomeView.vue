<template>
  <div class="home-view">
    <div class="home-header animate-slide-up">
      <div class="welcome-section">
        <h1 class="welcome-title">{{ $t('home.welcome') }}</h1>
        <p class="welcome-subtitle">{{ $t('home.subtitle') }}</p>
      </div>
      <el-button type="primary" size="large" :icon="Plus" @click="$router.push('/editor')">
        {{ $t('home.newTemplate') }}
      </el-button>
    </div>

    <div class="stats-grid animate-slide-up" style="animation-delay: 50ms">
      <div class="stat-card">
        <div class="stat-icon stat-icon--primary">
          <el-icon><Document /></el-icon>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ animatedStats.total }}</div>
          <div class="stat-label">{{ $t('home.totalTemplates') }}</div>
        </div>
        <div class="stat-trend stat-trend--up" v-if="stats.total > 0">
          <el-icon><TrendCharts /></el-icon>
          <span>{{ $t('home.active') }}</span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon stat-icon--success">
          <el-icon><CircleCheck /></el-icon>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ animatedStats.published }}</div>
          <div class="stat-label">{{ $t('home.published') }}</div>
        </div>
        <div class="stat-percentage" v-if="stats.total > 0">
          {{ Math.round((stats.published / stats.total) * 100) }}%
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon stat-icon--warning">
          <el-icon><Edit /></el-icon>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ animatedStats.draft }}</div>
          <div class="stat-label">{{ $t('home.draft') }}</div>
        </div>
        <div class="stat-badge" v-if="stats.draft > 0">
          {{ $t('home.pending') }}
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon stat-icon--info">
          <el-icon><Clock /></el-icon>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ animatedStats.todayEdited }}</div>
          <div class="stat-label">{{ $t('home.todayEdited') }}</div>
        </div>
        <div class="stat-trend stat-trend--neutral" v-if="stats.todayEdited > 0">
          <el-icon><Sunny /></el-icon>
          <span>{{ $t('home.today') }}</span>
        </div>
      </div>
    </div>

    <div class="content-grid animate-slide-up" style="animation-delay: 100ms">
      <div class="recent-section">
        <div class="section-header">
          <h2 class="section-title">
            <el-icon><Clock /></el-icon>
            {{ $t('home.recentEdited') }}
          </h2>
          <el-button text type="primary" @click="$router.push('/templates')">
            {{ $t('home.viewAll') }}
            <el-icon class="el-icon--right"><ArrowRight /></el-icon>
          </el-button>
        </div>

        <div class="recent-list" v-loading="loading">
          <template v-if="recentTemplates.length > 0">
            <div
              v-for="(template, index) in recentTemplates.slice(0, 5)"
              :key="template.id"
              class="recent-item"
              :style="{ animationDelay: `${150 + index * 50}ms` }"
              @click="$router.push(`/editor/${template.id}`)"
            >
              <div class="recent-item-icon">
                <el-icon><Document /></el-icon>
              </div>
              <div class="recent-item-content">
                <div class="recent-item-name">{{ template.name }}</div>
                <div class="recent-item-meta">
                  <span class="recent-item-type">{{ template.type }}</span>
                  <span class="recent-item-time">{{ formatRelativeTime(template.updateTime) }}</span>
                </div>
              </div>
              <div class="recent-item-status">
                <el-tag :type="template.isPublished ? 'success' : 'info'" size="small">
                  {{ template.isPublished ? $t('home.published') : $t('home.draft') }}
                </el-tag>
              </div>
              <el-icon class="recent-item-arrow"><ArrowRight /></el-icon>
            </div>
          </template>
          <div v-else class="empty-state">
            <div class="empty-icon">
              <el-icon><FolderOpened /></el-icon>
            </div>
            <p class="empty-text">{{ $t('home.noTemplates') }}</p>
            <el-button type="primary" :icon="Plus" @click="$router.push('/editor')">
              {{ $t('home.createFirst') }}
            </el-button>
          </div>
        </div>
      </div>

      <div class="quick-actions-section">
        <div class="section-header">
          <h2 class="section-title">
            <el-icon><Operation /></el-icon>
            {{ $t('home.quickActions') }}
          </h2>
        </div>

        <div class="quick-actions-grid">
          <div class="quick-action" @click="$router.push('/editor')">
            <div class="quick-action-icon">
              <el-icon><Plus /></el-icon>
            </div>
            <span class="quick-action-label">{{ $t('home.newTemplate') }}</span>
          </div>
          <div class="quick-action" @click="$router.push('/templates')">
            <div class="quick-action-icon">
              <el-icon><List /></el-icon>
            </div>
            <span class="quick-action-label">{{ $t('home.templateList') }}</span>
          </div>
          <div class="quick-action" @click="$router.push('/templates')">
            <div class="quick-action-icon">
              <el-icon><Search /></el-icon>
            </div>
            <span class="quick-action-label">{{ $t('home.searchTemplate') }}</span>
          </div>
          <div class="quick-action" @click="$router.push('/settings')">
            <div class="quick-action-icon">
              <el-icon><Setting /></el-icon>
            </div>
            <span class="quick-action-label">{{ $t('home.systemSettings') }}</span>
          </div>
        </div>

        <div class="tips-card">
          <div class="tips-header">
            <el-icon><InfoFilled /></el-icon>
            <span>{{ $t('home.tips') }}</span>
          </div>
          <ul class="tips-list">
            <li>{{ $t('home.tip1') }}</li>
            <li>{{ $t('home.tip2') }}</li>
            <li>{{ $t('home.tip3') }}</li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useTemplateStore } from '@/stores/template'
import $t from '@/locales/zh-CN'
import {
  Plus,
  Document,
  Clock,
  Edit,
  CircleCheck,
  TrendCharts,
  Sunny,
  ArrowRight,
  FolderOpened,
  Operation,
  List,
  Search,
  Setting,
  InfoFilled
} from '@element-plus/icons-vue'

const templateStore = useTemplateStore()
const loading = ref(false)
const recentTemplates = ref([])
const stats = reactive({ total: 0, published: 0, draft: 0, todayEdited: 0 })
const animatedStats = reactive({ total: 0, published: 0, draft: 0, todayEdited: 0 })

async function loadDashboard() {
  loading.value = true
  try {
    const result = await templateStore.fetchTemplates({ page: 1, pageSize: 10 })
    recentTemplates.value = result.items || []
    stats.total = result.totalCount || 0
    stats.published = recentTemplates.value.filter(t => t.isPublished).length
    stats.draft = stats.total - stats.published
    const today = new Date().toDateString()
    stats.todayEdited = recentTemplates.value.filter(t => new Date(t.updateTime).toDateString() === today).length

    animateNumbers()
  } catch (e) {
  } finally {
    loading.value = false
  }
}

function animateNumbers() {
  const duration = 800
  const steps = 30
  const interval = duration / steps

  Object.keys(stats).forEach(key => {
    const target = stats[key]
    const increment = target / steps
    let current = 0
    let step = 0

    const timer = setInterval(() => {
      step++
      current = Math.min(Math.round(increment * step), target)
      animatedStats[key] = current
      if (step >= steps) clearInterval(timer)
    }, interval)
  })
}

function formatRelativeTime(time) {
  if (!time) return ''
  const date = new Date(time)
  const now = new Date()
  const diff = now - date
  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)

  if (minutes < 1) return '刚刚'
  if (minutes < 60) return `${minutes}分钟前`
  if (hours < 24) return `${hours}小时前`
  if (days < 7) return `${days}天前`
  return date.toLocaleDateString('zh-CN')
}

onMounted(() => loadDashboard())
</script>

<style scoped>
.home-view {
  padding: var(--spacing-8);
  max-width: 1400px;
  margin: 0 auto;
}

.home-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--spacing-8);
}

.welcome-title {
  font-size: var(--font-size-4xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  margin: 0 0 var(--spacing-2) 0;
  letter-spacing: var(--letter-spacing-tight);
}

.welcome-subtitle {
  font-size: var(--font-size-base);
  color: var(--color-text-tertiary);
  margin: 0;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--spacing-5);
  margin-bottom: var(--spacing-8);
}

.stat-card {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-xl);
  padding: var(--spacing-5);
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-4);
  transition: all var(--transition-base);
  position: relative;
  overflow: hidden;
}

.stat-card:hover {
  border-color: var(--color-primary);
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-icon .el-icon {
  font-size: 24px;
}

.stat-icon--primary {
  background: var(--color-primary-bg);
  color: var(--color-primary);
}

.stat-icon--success {
  background: var(--color-success-bg);
  color: var(--color-success);
}

.stat-icon--warning {
  background: var(--color-warning-bg);
  color: var(--color-warning);
}

.stat-icon--info {
  background: var(--color-info-bg);
  color: var(--color-info);
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: var(--font-size-3xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  line-height: 1;
  margin-bottom: var(--spacing-1);
  font-family: var(--font-family-mono);
}

.stat-label {
  font-size: var(--font-size-sm);
  color: var(--color-text-tertiary);
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 8px;
  border-radius: var(--radius-full);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
}

.stat-trend--up {
  background: var(--color-success-bg);
  color: var(--color-success);
}

.stat-trend--neutral {
  background: var(--color-info-bg);
  color: var(--color-info);
}

.stat-percentage {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  color: var(--color-success);
  font-family: var(--font-family-mono);
}

.stat-badge {
  padding: 4px 10px;
  background: var(--color-warning-bg);
  color: var(--color-warning);
  border-radius: var(--radius-full);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 360px;
  gap: var(--spacing-6);
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--spacing-5);
}

.section-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  margin: 0;
}

.section-title .el-icon {
  color: var(--color-primary);
}

.recent-section {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-xl);
  padding: var(--spacing-6);
}

.recent-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-2);
  min-height: 300px;
}

.recent-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-4);
  padding: var(--spacing-4);
  border-radius: var(--radius-lg);
  cursor: pointer;
  transition: all var(--transition-fast);
  animation: slideUp var(--transition-slow) ease-out both;
}

.recent-item:hover {
  background: var(--color-bg-hover);
}

.recent-item-icon {
  width: 40px;
  height: 40px;
  background: var(--color-primary-bg);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  flex-shrink: 0;
}

.recent-item-content {
  flex: 1;
  min-width: 0;
}

.recent-item-name {
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
  margin-bottom: 2px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.recent-item-meta {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
  font-size: var(--font-size-sm);
  color: var(--color-text-tertiary);
}

.recent-item-type {
  padding: 2px 8px;
  background: var(--color-bg-tertiary);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
}

.recent-item-status {
  flex-shrink: 0;
}

.recent-item-arrow {
  color: var(--color-text-muted);
  opacity: 0;
  transition: opacity var(--transition-fast);
}

.recent-item:hover .recent-item-arrow {
  opacity: 1;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-12) var(--spacing-6);
  text-align: center;
}

.empty-icon {
  width: 64px;
  height: 64px;
  background: var(--color-bg-tertiary);
  border-radius: var(--radius-xl);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--spacing-4);
}

.empty-icon .el-icon {
  font-size: 28px;
  color: var(--color-text-muted);
}

.empty-text {
  font-size: var(--font-size-base);
  color: var(--color-text-tertiary);
  margin: 0 0 var(--spacing-4) 0;
}

.quick-actions-section {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-6);
}

.quick-actions-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--spacing-3);
}

.quick-action {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-lg);
  padding: var(--spacing-5);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-3);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.quick-action:hover {
  border-color: var(--color-primary);
  background: var(--color-primary-bg);
}

.quick-action-icon {
  width: 44px;
  height: 44px;
  background: var(--color-bg-tertiary);
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  transition: all var(--transition-fast);
}

.quick-action:hover .quick-action-icon {
  background: var(--color-primary);
  color: var(--color-text-on-primary);
}

.quick-action-label {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
}

.tips-card {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-xl);
  padding: var(--spacing-5);
}

.tips-header {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  margin-bottom: var(--spacing-4);
}

.tips-header .el-icon {
  color: var(--color-info);
}

.tips-list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-3);
}

.tips-list li {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  padding-left: var(--spacing-4);
  position: relative;
}

.tips-list li::before {
  content: '';
  position: absolute;
  left: 0;
  top: 8px;
  width: 4px;
  height: 4px;
  background: var(--color-primary);
  border-radius: 50%;
}

@media (max-width: 1200px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .content-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .home-view {
    padding: var(--spacing-4);
  }

  .home-header {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--spacing-4);
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }
}
</style>
