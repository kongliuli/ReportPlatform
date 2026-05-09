<template>
  <div class="toolbar">
    <div class="toolbar-section toolbar-left">
      <el-button :icon="Back" @click="$router.back()" text class="toolbar-btn">
        <span class="btn-text">返回</span>
      </el-button>
      <div class="toolbar-divider"></div>
      <div class="undo-redo-group">
        <el-tooltip :content="undoTooltip" placement="bottom">
          <button
            class="icon-btn"
            :disabled="!canUndo"
            @click="$emit('undo')"
          >
            <el-icon><RefreshLeft /></el-icon>
          </button>
        </el-tooltip>
        <el-tooltip :content="redoTooltip" placement="bottom">
          <button
            class="icon-btn"
            :disabled="!canRedo"
            @click="$emit('redo')"
          >
            <el-icon><RefreshRight /></el-icon>
          </button>
        </el-tooltip>
      </div>
    </div>

    <div class="toolbar-section toolbar-center">
      <div class="zoom-control">
        <el-tooltip content="缩小" placement="bottom">
          <button class="icon-btn" @click="$emit('zoom-out')">
            <el-icon><ZoomOut /></el-icon>
          </button>
        </el-tooltip>
        <span class="zoom-value">{{ zoomPercentage }}%</span>
        <el-tooltip content="放大" placement="bottom">
          <button class="icon-btn" @click="$emit('zoom-in')">
            <el-icon><ZoomIn /></el-icon>
          </button>
        </el-tooltip>
        <el-tooltip content="适应屏幕" placement="bottom">
          <button class="icon-btn" @click="$emit('fit-screen')">
            <el-icon><FullScreen /></el-icon>
          </button>
        </el-tooltip>
      </div>
      <div class="toolbar-divider"></div>
      <div class="view-controls">
        <el-tooltip :content="showGrid ? '隐藏网格' : '显示网格'" placement="bottom">
          <button
            class="icon-btn"
            :class="{ active: showGrid }"
            @click="$emit('toggle-grid')"
          >
            <el-icon><Grid /></el-icon>
          </button>
        </el-tooltip>
        <el-tooltip content="页面设置" placement="bottom">
          <button class="icon-btn" @click="openPageSettings">
            <el-icon><Document /></el-icon>
          </button>
        </el-tooltip>
      </div>
    </div>

    <div class="toolbar-section toolbar-right">
      <el-button text class="toolbar-btn" @click="$emit('export-json')">
        <el-icon><Download /></el-icon>
        <span class="btn-text">导出JSON</span>
      </el-button>
      <el-button class="preview-btn" @click="$emit('preview')">
        <el-icon><View /></el-icon>
        <span>预览</span>
      </el-button>
      <el-button type="primary" class="save-btn" @click="$emit('save')">
        <el-icon><Check /></el-icon>
        <span>保存</span>
      </el-button>
    </div>

    <el-dialog
      v-model="showPageSettings"
      title="页面设置"
      width="480px"
      :close-on-click-modal="false"
      class="page-settings-dialog"
    >
      <el-form label-position="top" size="default" class="settings-form">
        <el-form-item label="纸张大小">
          <el-radio-group v-model="pageForm.size" @change="onSizeChange" class="size-radio-group">
            <el-radio-button value="A4">
              <div class="size-option">
                <span class="size-name">A4</span>
                <span class="size-dims">210 × 297 mm</span>
              </div>
            </el-radio-button>
            <el-radio-button value="A5">
              <div class="size-option">
                <span class="size-name">A5</span>
                <span class="size-dims">148 × 210 mm</span>
              </div>
            </el-radio-button>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="纸张方向">
          <el-radio-group v-model="pageForm.orientation" class="orientation-radio-group">
            <el-radio-button value="Portrait">
              <div class="orientation-option">
                <div class="orientation-icon portrait"></div>
                <span>纵向</span>
              </div>
            </el-radio-button>
            <el-radio-button value="Landscape">
              <div class="orientation-option">
                <div class="orientation-icon landscape"></div>
                <span>横向</span>
              </div>
            </el-radio-button>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="页边距 (mm)">
          <div class="margin-inputs">
            <div class="margin-input-group">
              <el-input-number
                v-model="pageForm.marginTop"
                :min="0"
                :max="50"
                controls-position="right"
                size="small"
              />
              <span class="margin-label">上</span>
            </div>
            <div class="margin-row">
              <div class="margin-input-group">
                <el-input-number
                  v-model="pageForm.marginLeft"
                  :min="0"
                  :max="50"
                  controls-position="right"
                  size="small"
                />
                <span class="margin-label">左</span>
              </div>
              <div class="margin-input-group">
                <el-input-number
                  v-model="pageForm.marginRight"
                  :min="0"
                  :max="50"
                  controls-position="right"
                  size="small"
                />
                <span class="margin-label">右</span>
              </div>
            </div>
            <div class="margin-input-group">
              <el-input-number
                v-model="pageForm.marginBottom"
                :min="0"
                :max="50"
                controls-position="right"
                size="small"
              />
              <span class="margin-label">下</span>
            </div>
          </div>
        </el-form-item>

        <el-form-item label="预览">
          <div class="page-preview">
            <div class="page-preview-paper" :style="previewStyle">
              <div class="page-preview-margin" :style="marginStyle"></div>
            </div>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="showPageSettings = false">取消</el-button>
          <el-button type="primary" @click="applyPageSettings">应用</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, reactive } from 'vue'
import {
  Back,
  RefreshLeft,
  RefreshRight,
  ZoomIn,
  ZoomOut,
  FullScreen,
  Grid,
  Check,
  Document,
  Download,
  View
} from '@element-plus/icons-vue'
import { ElMessageBox, ElMessage } from 'element-plus'
import { useTemplateStore } from '@/stores/template'
import { useEditorStore } from '@/stores/editor'

defineEmits(['save', 'preview', 'undo', 'redo', 'zoom-in', 'zoom-out', 'fit-screen', 'toggle-grid', 'export-json'])

const templateStore = useTemplateStore()
const editorStore = useEditorStore()

const canUndo = computed(() => templateStore.canUndo)
const canRedo = computed(() => templateStore.canRedo)
const undoTooltip = computed(() => {
  const desc = templateStore.lastUndoDescription
  return `撤销${desc ? ': ' + desc : ''} (Ctrl+Z)`
})
const redoTooltip = computed(() => {
  const desc = templateStore.lastRedoDescription
  return `重做${desc ? ': ' + desc : ''} (Ctrl+Y)`
})
const zoomPercentage = computed(() => editorStore.zoomPercentage)
const showGrid = computed(() => editorStore.showGrid)

const PAGE_SIZES = {
  A4: { width: 210, height: 297 },
  A5: { width: 148, height: 210 }
}

const showPageSettings = ref(false)
const pageForm = reactive({
  size: 'A4',
  orientation: 'Portrait',
  marginLeft: 10,
  marginRight: 10,
  marginTop: 10,
  marginBottom: 10
})

function detectPageSize(w, h) {
  if (Math.abs(w - 210) < 1 && Math.abs(h - 297) < 1) return { name: 'A4', orientation: 'Portrait' }
  if (Math.abs(w - 297) < 1 && Math.abs(h - 210) < 1) return { name: 'A4', orientation: 'Landscape' }
  if (Math.abs(w - 148) < 1 && Math.abs(h - 210) < 1) return { name: 'A5', orientation: 'Portrait' }
  if (Math.abs(w - 210) < 1 && Math.abs(h - 148) < 1) return { name: 'A5', orientation: 'Landscape' }
  return { name: '自定义', orientation: h > w ? 'Portrait' : 'Landscape' }
}

const previewStyle = computed(() => {
  const base = PAGE_SIZES[pageForm.size]
  const isLandscape = pageForm.orientation === 'Landscape'
  const w = isLandscape ? base.height : base.width
  const h = isLandscape ? base.width : base.height
  const scale = 140 / Math.max(w, h)
  return {
    width: `${w * scale}px`,
    height: `${h * scale}px`
  }
})

const marginStyle = computed(() => {
  const base = PAGE_SIZES[pageForm.size]
  const isLandscape = pageForm.orientation === 'Landscape'
  const w = isLandscape ? base.height : base.width
  const h = isLandscape ? base.width : base.height
  const scale = 140 / Math.max(w, h)
  return {
    left: `${pageForm.marginLeft * scale}px`,
    right: `${pageForm.marginRight * scale}px`,
    top: `${pageForm.marginTop * scale}px`,
    bottom: `${pageForm.marginBottom * scale}px`
  }
})

function onSizeChange() {}

function openPageSettings() {
  const tmpl = templateStore.currentTemplate
  if (tmpl) {
    const detected = detectPageSize(tmpl.pageWidth, tmpl.pageHeight)
    if (PAGE_SIZES[detected.name]) {
      pageForm.size = detected.name
    }
    pageForm.orientation = tmpl.orientation || detected.orientation
    pageForm.marginLeft = tmpl.marginLeft ?? 10
    pageForm.marginRight = tmpl.marginRight ?? 10
    pageForm.marginTop = tmpl.marginTop ?? 10
    pageForm.marginBottom = tmpl.marginBottom ?? 10
  }
  showPageSettings.value = true
}

async function applyPageSettings() {
  const base = PAGE_SIZES[pageForm.size]
  const isLandscape = pageForm.orientation === 'Landscape'
  const newWidth = isLandscape ? base.height : base.width
  const newHeight = isLandscape ? base.width : base.height

  const tmpl = templateStore.currentTemplate
  const hasElements = tmpl?.elements?.length > 0

  if (hasElements) {
    try {
      await ElMessageBox.confirm(
        '切换页面设置可能导致部分元素超出页面范围，是否继续？',
        '页面设置变更',
        { confirmButtonText: '继续', cancelButtonText: '取消', type: 'warning' }
      )
    } catch {
      return
    }
  }

  templateStore.updatePageSettings({
    pageWidth: newWidth,
    pageHeight: newHeight,
    orientation: pageForm.orientation,
    marginLeft: pageForm.marginLeft,
    marginRight: pageForm.marginRight,
    marginTop: pageForm.marginTop,
    marginBottom: pageForm.marginBottom
  })

  showPageSettings.value = false
  ElMessage.success('页面设置已更新')
}
</script>

<style scoped>
.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 56px;
  padding: 0 var(--spacing-4);
  background: var(--color-bg-secondary);
  border-bottom: 1px solid var(--color-border-light);
}

.toolbar-section {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.toolbar-divider {
  width: 1px;
  height: 24px;
  background: var(--color-border-light);
  margin: 0 var(--spacing-2);
}

.toolbar-btn {
  color: var(--color-text-secondary);
}

.toolbar-btn:hover {
  color: var(--color-primary);
}

.btn-text {
  margin-left: var(--spacing-1);
}

.undo-redo-group {
  display: flex;
  align-items: center;
  gap: var(--spacing-1);
}

.icon-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  border-radius: var(--radius-md);
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.icon-btn:hover:not(:disabled) {
  background: var(--color-bg-hover);
  color: var(--color-primary);
}

.icon-btn:disabled {
  color: var(--color-text-muted);
  cursor: not-allowed;
}

.icon-btn.active {
  background: var(--color-primary-bg);
  color: var(--color-primary);
}

.zoom-control {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.zoom-value {
  min-width: 48px;
  text-align: center;
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
  font-family: var(--font-family-mono);
}

.view-controls {
  display: flex;
  align-items: center;
  gap: var(--spacing-1);
}

.preview-btn {
  background: var(--color-bg-tertiary);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
}

.preview-btn:hover {
  background: var(--color-bg-hover);
  border-color: var(--color-primary);
  color: var(--color-primary);
}

.save-btn {
  font-weight: var(--font-weight-medium);
}

.settings-form :deep(.el-form-item__label) {
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
}

.size-radio-group :deep(.el-radio-button__inner) {
  padding: var(--spacing-3) var(--spacing-5);
}

.size-option {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
}

.size-name {
  font-weight: var(--font-weight-semibold);
}

.size-dims {
  font-size: var(--font-size-xs);
  color: var(--color-text-tertiary);
}

.orientation-radio-group :deep(.el-radio-button__inner) {
  padding: var(--spacing-4) var(--spacing-6);
}

.orientation-option {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-2);
}

.orientation-icon {
  width: 24px;
  background: var(--color-bg-tertiary);
  border: 1px solid var(--color-border);
  border-radius: 2px;
}

.orientation-icon.portrait {
  height: 32px;
}

.orientation-icon.landscape {
  height: 18px;
}

.margin-inputs {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-3);
}

.margin-row {
  display: flex;
  gap: var(--spacing-8);
}

.margin-input-group {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--spacing-1);
}

.margin-input-group :deep(.el-input-number) {
  width: 80px;
}

.margin-label {
  font-size: var(--font-size-xs);
  color: var(--color-text-tertiary);
}

.page-preview {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: var(--spacing-6);
  background: var(--color-bg-tertiary);
  border-radius: var(--radius-lg);
}

.page-preview-paper {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border);
  position: relative;
  box-shadow: var(--shadow-sm);
}

.page-preview-margin {
  position: absolute;
  border: 1px dashed var(--color-primary);
  background: var(--color-primary-bg);
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-3);
}
</style>
