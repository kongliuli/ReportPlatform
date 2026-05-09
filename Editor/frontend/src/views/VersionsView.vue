<template>
  <div class="versions-layout">
    <header class="top-navbar">
      <div class="navbar-left">
        <span class="brand-text" @click="$router.push('/')">Stitch</span>
        <span class="brand-tag">PDF 模板设计器</span>
        <nav class="nav-tabs">
          <a class="nav-tab" @click="$router.push('/')">模板管理</a>
          <a class="nav-tab active">版本追溯</a>
          <a class="nav-tab" @click="$router.push('/templates')">数据源</a>
          <a class="nav-tab" @click="$router.push('/editor')">编辑器</a>
        </nav>
      </div>
      <div class="navbar-right">
        <div class="search-box">
          <span class="material-symbols-outlined sm">search</span>
          <input type="text" placeholder="搜索版本..." class="search-input" />
        </div>
        <div class="nav-icons">
          <span class="material-symbols-outlined nav-icon">help</span>
          <span class="material-symbols-outlined nav-icon">notifications</span>
          <span class="material-symbols-outlined nav-icon">settings</span>
        </div>
        <div class="avatar">
          <span class="material-symbols-outlined">account_circle</span>
        </div>
      </div>
    </header>

    <div class="version-controls">
      <div class="version-selectors">
        <div class="version-selector">
          <span class="xl-font-label-caps" style="color: var(--color-outline)">对比版本 A</span>
          <div class="version-tag primary">
            <span class="material-symbols-outlined sm">history</span>
            v2.4.1 (当前)
          </div>
        </div>
        <span class="material-symbols-outlined" style="color: var(--color-outline)">compare_arrows</span>
        <div class="version-selector">
          <span class="xl-font-label-caps" style="color: var(--color-outline)">对比版本 B</span>
          <div class="version-tag secondary">
            <span class="material-symbols-outlined sm">restore</span>
            v2.3.9
          </div>
        </div>
      </div>
      <div class="version-actions">
        <div class="filter-checks">
          <label class="filter-check">
            <input type="checkbox" checked />
            <span class="xl-font-body-sm">显示新增内容</span>
          </label>
          <label class="filter-check">
            <input type="checkbox" checked />
            <span class="xl-font-body-sm">显示删除内容</span>
          </label>
        </div>
        <button class="xl-btn-primary" style="padding: 8px 16px">
          <span class="material-symbols-outlined sm">file_download</span>
          导出差异报告
        </button>
      </div>
    </div>

    <main class="versions-main">
      <aside class="timeline-sidebar custom-scrollbar">
        <h2 class="xl-font-h2" style="margin-bottom: 24px">版本历史记录</h2>
        <div class="timeline">
          <div v-for="(ver, idx) in versionHistory" :key="idx" class="timeline-entry" :class="{ active: idx === 0, compare: idx === 2 }">
            <div class="timeline-marker" :class="ver.markerClass"></div>
            <div class="timeline-content">
              <div class="timeline-header">
                <span class="xl-font-mono-data" style="font-weight: 700" :style="{ color: idx === 0 ? 'var(--color-xinglin-blue)' : 'var(--color-on-surface)' }">{{ ver.version }}</span>
                <span class="xl-font-label-caps" style="color: var(--color-outline)">{{ ver.date }}</span>
              </div>
              <p class="xl-font-body-sm" :style="{ color: idx === 0 ? 'var(--color-on-surface)' : 'var(--color-outline)' }">{{ ver.desc }}</p>
              <span v-if="ver.badge" class="timeline-badge" :class="ver.badgeClass">{{ ver.badge }}</span>
            </div>
          </div>
        </div>
      </aside>

      <section class="diff-canvas custom-scrollbar">
        <div class="diff-document">
          <div class="diff-content">
            <div class="diff-section-header">
              <h1 style="font-size: 32px; font-weight: 700; letter-spacing: -0.02em; color: #1e293b">商业发票模板变更说明</h1>
              <p class="xl-font-mono-data" style="color: var(--color-outline); margin-top: 8px">TEMPLATE ID: TPL-INV-2024-001</p>
            </div>

            <div class="diff-section">
              <h2 class="xl-font-h1" style="border-left: 4px solid var(--color-xinglin-blue); padding-left: 16px">1. 发票明细计算逻辑</h2>
              <p class="xl-font-body-md" style="color: var(--color-on-surface-variant); line-height: 1.6; margin-top: 12px">
                在 V2.4.1 版本中，我们对金额自动计算逻辑进行了优化。系统将根据商品单价和数量自动计算小计，并在税额变化时触发实时更新。
              </p>
              <div class="diff-added" style="padding: 16px; border-radius: var(--radius-lg); margin-top: 16px; position: relative">
                <span class="diff-label added">新增</span>
                <p class="xl-font-body-md" style="font-weight: 500; color: #15803d">
                  3.2.1 多币种支持：对于跨境发票，系统现在支持实时汇率换算，并自动在发票底部显示等值金额。
                </p>
              </div>
            </div>

            <div class="diff-section">
              <h2 class="xl-font-h1" style="border-left: 4px solid var(--color-xinglin-blue); padding-left: 16px">2. 数据源自动同步</h2>
              <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 32px; margin-top: 16px">
                <div style="border: 1px solid var(--color-outline-variant); padding: 16px; border-radius: var(--radius-default); background: var(--color-surface-container-low); opacity: 0.6">
                  <h3 class="xl-font-label-caps" style="margin-bottom: 8px">原始逻辑 (v2.3.9)</h3>
                  <p class="xl-font-body-sm" style="font-style: italic">手动触发 API 数据刷新</p>
                </div>
                <div style="border: 2px solid var(--color-xinglin-blue); border-style: dashed; padding: 16px; border-radius: var(--radius-default); background: rgba(0, 85, 255, 0.05)">
                  <h3 class="xl-font-label-caps" style="margin-bottom: 8px; color: var(--color-xinglin-blue)">更新逻辑 (v2.4.1)</h3>
                  <p class="xl-font-body-sm" style="font-weight: 600">WebSocket 实时推送，秒级更新</p>
                </div>
              </div>
              <div class="diff-removed" style="padding: 16px; border-radius: var(--radius-lg); margin-top: 16px; position: relative; opacity: 0.5">
                <span class="diff-label removed">删除</span>
                <p class="xl-font-body-sm" style="color: #dc2626; text-decoration: line-through">
                  过时备注：请在每次生成 PDF 前手动点击右上角的"同步数据"按钮以确保数据最新。
                </p>
              </div>
            </div>

            <div class="diff-section">
              <h2 class="xl-font-h1" style="border-left: 4px solid var(--color-xinglin-blue); padding-left: 16px">3. 签名与合规性</h2>
              <p class="xl-font-body-md" style="color: var(--color-on-surface-variant); margin-top: 12px">所有生成的 PDF 文档必须带有数字签名。在本次更新中，我们集成了最新的 CA 证书接口协议。</p>
            </div>
          </div>

          <div class="watermark">Internal Diff Only</div>
        </div>
      </section>

      <aside class="stats-sidebar custom-scrollbar">
        <section class="stats-section">
          <h3 class="xl-font-h2" style="margin-bottom: 16px">变更统计</h3>
          <div class="stats-list">
            <div class="stat-item added">
              <div class="stat-item-left">
                <span class="material-symbols-outlined">add_circle</span>
                <span class="xl-font-h3">新增字段</span>
              </div>
              <span class="xl-font-mono-data" style="font-size: 20px; font-weight: 700; color: #15803d">12</span>
            </div>
            <div class="stat-item removed">
              <div class="stat-item-left">
                <span class="material-symbols-outlined">remove_circle</span>
                <span class="xl-font-h3">删除项</span>
              </div>
              <span class="xl-font-mono-data" style="font-size: 20px; font-weight: 700; color: #dc2626">4</span>
            </div>
            <div class="stat-item modified">
              <div class="stat-item-left">
                <span class="material-symbols-outlined">edit_square</span>
                <span class="xl-font-h3">逻辑修改</span>
              </div>
              <span class="xl-font-mono-data" style="font-size: 20px; font-weight: 700; color: #1d4ed8">28</span>
            </div>
          </div>
        </section>

        <section class="stats-section">
          <h3 class="xl-font-h2" style="margin-bottom: 16px">数据绑定变更详情</h3>
          <div class="binding-list">
            <div v-for="change in bindingChanges" :key="change.id" class="binding-item">
              <div class="binding-header">
                <span class="xl-font-label-caps" style="color: var(--color-outline)">{{ change.id }}</span>
                <span class="change-badge" :class="change.badgeClass">{{ change.badge }}</span>
              </div>
              <p class="xl-font-body-sm" style="font-weight: 600; margin-bottom: 4px">{{ change.field }}</p>
              <div v-if="change.diff" class="code-diff">
                <div v-for="line in change.diff" :key="line.text" class="diff-line" :class="line.type">
                  <span class="diff-prefix">{{ line.prefix }}</span>
                  <span class="xl-font-mono-data" :style="{ color: line.color }">{{ line.text }}</span>
                </div>
              </div>
              <p v-if="change.binding" class="xl-font-body-sm" style="color: var(--color-outline)">
                绑定至：<span class="xl-font-mono-data">{{ change.binding }}</span>
              </p>
            </div>
          </div>
        </section>

        <div style="margin-top: auto; padding-top: 24px; border-top: 1px solid var(--color-surface-container)">
          <button class="xl-btn-secondary" style="width: 100%; padding: 10px">
            <span class="material-symbols-outlined sm">forum</span>
            查看审核意见 (3)
          </button>
        </div>
      </aside>
    </main>

    <div class="fab-group">
      <button class="fab-btn secondary">
        <span class="material-symbols-outlined">fullscreen</span>
      </button>
      <button class="fab-btn primary">
        <span class="material-symbols-outlined filled">check_circle</span>
      </button>
    </div>
  </div>
</template>

<script setup>
const versionHistory = [
  { version: 'v2.4.1', date: '今日 14:20', desc: '更新了发票模板的金额计算逻辑', markerClass: 'active', badge: '当前选中', badgeClass: 'badge-active' },
  { version: 'v2.4.0', date: '昨日 09:15', desc: '修复了多币种换算的精度 Bug', markerClass: 'past' },
  { version: 'v2.3.9', date: '03-24', desc: '发布正式版本 - 发票模板库 V2 升级', markerClass: 'compare', badge: '已选对比', badgeClass: 'badge-compare' },
  { version: 'v2.3.8', date: '03-20', desc: '初始化同步脚本...', markerClass: 'old' }
]

const bindingChanges = [
  {
    id: 'FIELD_UUID: 9283',
    badge: 'Updated',
    badgeClass: 'badge-updated',
    field: 'SUBTOTAL_CALC_LOGIC',
    diff: [
      { prefix: '-', text: '$(qty) * $(price)', type: 'removed', color: '#dc2626' },
      { prefix: '+', text: 'round($(qty) * $(price), 2)', type: 'added', color: '#15803d' }
    ]
  },
  {
    id: 'FIELD_UUID: 1042',
    badge: 'New',
    badgeClass: 'badge-new',
    field: 'CURRENCY_EXCHANGE',
    binding: 'api.fx.rates'
  },
  {
    id: 'FIELD_UUID: 0042',
    badge: 'Removed',
    badgeClass: 'badge-removed',
    field: 'LEGACY_SYNC_BTN'
  }
]
</script>

<style scoped>
.versions-layout {
  height: 100vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: var(--color-background);
}

.top-navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 24px;
  height: var(--navbar-height);
  background: var(--color-surface-container-lowest);
  border-bottom: 1px solid var(--color-outline-variant);
  flex-shrink: 0;
  z-index: 50;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
}

.navbar-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.brand-text {
  font-size: 20px;
  font-weight: 900;
  color: var(--color-xinglin-blue);
  letter-spacing: -0.03em;
  cursor: pointer;
}

.brand-tag {
  font-size: 11px;
  font-weight: 600;
  color: var(--color-outline);
  background: var(--color-surface-container);
  padding: 2px 8px;
  border-radius: var(--radius-default);
  letter-spacing: 0.02em;
}

.nav-tabs {
  display: flex;
  gap: 24px;
  height: var(--navbar-height);
  align-items: center;
  margin-left: 16px;
}

.nav-tab {
  font-size: 14px;
  font-weight: 500;
  color: var(--color-on-surface-variant);
  cursor: pointer;
  padding-bottom: 16px;
  margin-bottom: -17px;
  border-bottom: 2px solid transparent;
  transition: color 0.2s;
}

.nav-tab:hover {
  color: var(--color-xinglin-blue);
}

.nav-tab.active {
  color: var(--color-xinglin-blue);
  border-bottom-color: var(--color-xinglin-blue);
}

.navbar-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
}

.search-box .material-symbols-outlined {
  position: absolute;
  left: 8px;
  color: var(--color-outline);
}

.search-input {
  padding-left: 32px;
  padding-right: 16px;
  padding-top: 6px;
  padding-bottom: 6px;
  background: var(--color-surface-container);
  border: none;
  border-radius: var(--radius-lg);
  font-size: 12px;
  width: 192px;
  outline: none;
}

.nav-icons {
  display: flex;
  gap: 12px;
}

.nav-icon {
  color: var(--color-on-surface-variant);
  cursor: pointer;
  transition: color 0.15s;
}

.nav-icon:hover {
  color: var(--color-xinglin-blue);
}

.avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 1px solid var(--color-outline-variant);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-outline);
  cursor: pointer;
}

.version-controls {
  position: fixed;
  top: var(--navbar-height);
  left: 0;
  right: 0;
  height: 64px;
  background: var(--color-surface-container-lowest);
  border-bottom: 1px solid var(--color-outline-variant);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  z-index: 40;
}

.version-selectors {
  display: flex;
  align-items: center;
  gap: 16px;
}

.version-selector {
  display: flex;
  flex-direction: column;
}

.version-tag {
  display: flex;
  align-items: center;
  gap: 8px;
  font-family: var(--font-family-mono);
  font-weight: 700;
}

.version-tag.primary {
  color: var(--color-xinglin-blue);
}

.version-tag.secondary {
  color: var(--color-secondary);
}

.version-actions {
  display: flex;
  align-items: center;
  gap: 24px;
}

.filter-checks {
  display: flex;
  align-items: center;
  gap: 16px;
  padding-right: 24px;
  border-right: 1px solid var(--color-outline-variant);
}

.filter-check {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.versions-main {
  display: flex;
  flex: 1;
  padding-top: 120px;
  overflow: hidden;
}

.timeline-sidebar {
  width: var(--sidebar-width);
  flex-shrink: 0;
  border-right: 1px solid var(--color-outline-variant);
  background: var(--color-surface-container-lowest);
  overflow-y: auto;
  padding: 24px;
}

.timeline {
  position: relative;
  border-left: 2px solid var(--color-surface-container);
  margin-left: 8px;
}

.timeline-entry {
  position: relative;
  padding-left: 24px;
  padding-bottom: 32px;
}

.timeline-marker {
  position: absolute;
  left: -9px;
  top: 4px;
  width: 16px;
  height: 16px;
  border-radius: 50%;
}

.timeline-marker.active {
  background: var(--color-xinglin-blue);
  box-shadow: 0 0 0 4px rgba(0, 85, 255, 0.1);
}

.timeline-marker.past {
  background: #cbd5e1;
}

.timeline-marker.compare {
  background: var(--color-xinglin-blue);
  border: 2px solid white;
  box-shadow: 0 0 0 2px rgba(0, 85, 255, 0.2);
}

.timeline-marker.old {
  background: #e2e8f0;
}

.timeline-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.timeline-badge {
  display: inline-block;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  padding: 2px 6px;
  border-radius: var(--radius-default);
  margin-top: 4px;
}

.badge-active {
  background: var(--color-secondary-container);
  color: var(--color-on-secondary-container);
}

.badge-compare {
  background: rgba(182, 196, 255, 0.3);
  color: var(--color-xinglin-blue);
}

.diff-canvas {
  flex: 1;
  background: var(--color-surface-container);
  overflow-y: auto;
  padding: 48px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.diff-document {
  width: 840px;
  background: var(--color-surface-container-lowest);
  box-shadow: var(--shadow-level-3);
  min-height: 1100px;
  padding: 64px;
  position: relative;
}

.diff-content {
  display: flex;
  flex-direction: column;
  gap: 48px;
}

.diff-section-header {
  text-align: center;
  border-bottom: 1px solid var(--color-outline-variant);
  padding-bottom: 32px;
}

.diff-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.diff-label {
  position: absolute;
  top: -12px;
  left: 16px;
  font-size: 10px;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: var(--radius-default);
  color: white;
}

.diff-label.added {
  background: #22c55e;
}

.diff-label.removed {
  background: #ef4444;
}

.diff-line {
  display: flex;
  gap: 8px;
  font-size: 12px;
}

.diff-prefix {
  width: 16px;
  font-weight: 700;
  flex-shrink: 0;
}

.watermark {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0.03;
  pointer-events: none;
  transform: rotate(-45deg);
  font-size: 120px;
  font-weight: 900;
  text-transform: uppercase;
}

.stats-sidebar {
  width: var(--inspector-width);
  flex-shrink: 0;
  border-left: 1px solid var(--color-outline-variant);
  background: var(--color-surface-container-lowest);
  overflow-y: auto;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.stats-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.stat-item {
  padding: 12px;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.stat-item.added {
  background: #f0fdf4;
  border: 1px solid #dcfce7;
  color: #15803d;
}

.stat-item.removed {
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
}

.stat-item.modified {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  color: #1d4ed8;
}

.stat-item-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.binding-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.binding-item {
  border: 1px solid var(--color-surface-container);
  padding: 12px;
  border-radius: var(--radius-lg);
  transition: border-color 0.15s;
}

.binding-item:hover {
  border-color: var(--color-xinglin-blue);
}

.binding-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.change-badge {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: var(--radius-full);
  font-weight: 600;
}

.badge-updated {
  background: var(--color-primary-fixed);
  color: var(--color-xinglin-blue);
}

.badge-new {
  background: #dcfce7;
  color: #15803d;
}

.badge-removed {
  background: var(--color-surface-container);
  color: var(--color-on-surface-variant);
}

.code-diff {
  display: flex;
  flex-direction: column;
  gap: 2px;
  font-size: 12px;
  margin-top: 4px;
}

.fab-group {
  position: fixed;
  bottom: 32px;
  right: 32px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.fab-btn {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  cursor: pointer;
  transition: transform 0.15s, box-shadow 0.15s;
  box-shadow: var(--shadow-level-3);
}

.fab-btn:hover {
  transform: scale(1.05);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
}

.fab-btn.primary {
  background: var(--color-xinglin-blue);
  color: white;
}

.fab-btn.secondary {
  background: var(--color-surface-container-lowest);
  color: var(--color-outline);
}

.fab-btn.secondary:hover {
  color: var(--color-xinglin-blue);
}
</style>
