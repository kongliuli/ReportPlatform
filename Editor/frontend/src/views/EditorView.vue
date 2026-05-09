<template>
  <div class="editor-view">
    <Toolbar
      @save="handleSave"
      @preview="handlePreview"
      @undo="handleUndo"
      @redo="handleRedo"
      @zoom-in="handleZoomIn"
      @zoom-out="handleZoomOut"
      @fit-screen="handleFitScreen"
      @toggle-grid="handleToggleGrid"
      @export-json="handleExportJson"
    />
    <div class="editor-body">
      <Toolbox
        :collapsed="editorStore.leftPanelCollapsed"
        @add-element="handleAddElement"
        @toggle-collapse="editorStore.toggleLeftPanel()"
      />
      <CanvasArea />
      <div class="right-panel" :class="{ collapsed: editorStore.rightPanelCollapsed }">
        <div class="right-panel-toggle" @click="editorStore.toggleRightPanel()">
          <el-icon :size="14">
            <component :is="editorStore.rightPanelCollapsed ? Expand : Fold" />
          </el-icon>
        </div>
        <template v-if="!editorStore.rightPanelCollapsed">
          <el-tabs v-model="rightTab" class="right-tabs">
            <el-tab-pane label="属性" name="props">
              <PropertyPanel />
            </el-tab-pane>
            <el-tab-pane label="图层" name="layers">
              <LayerPanel />
            </el-tab-pane>
          </el-tabs>
        </template>
      </div>
    </div>
    <PreviewModal :visible="showPreview" :template-id="templateId" @close="showPreview = false" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Expand, Fold } from '@element-plus/icons-vue'
import { useTemplateStore } from '@/stores/template'
import { useEditorStore } from '@/stores/editor'
import { useTemplate } from '@/composables/useTemplate'
import Toolbar from '@/components/editor/Toolbar.vue'
import Toolbox from '@/components/editor/Toolbox.vue'
import CanvasArea from '@/components/editor/CanvasArea.vue'
import PropertyPanel from '@/components/editor/PropertyPanel.vue'
import LayerPanel from '@/components/editor/LayerPanel.vue'
import PreviewModal from '@/components/preview/PreviewModal.vue'

const route = useRoute()
const templateStore = useTemplateStore()
const editorStore = useEditorStore()
const { loadTemplate, saveTemplate, undo, redo } = useTemplate()

const rightTab = ref('props')
const showPreview = ref(false)
const templateId = computed(() => route.params.id)

onMounted(async () => {
  const id = route.params.id
  if (id) {
    try {
      await loadTemplate(id)
    } catch (e) {
      ElMessage.error('加载模板失败')
    }
  }
})

function handleAddElement(type) {
  if (!templateStore.currentTemplate) return
  templateStore.addElement(type)
}

async function handleSave() {
  await saveTemplate()
}

function handlePreview() {
  showPreview.value = true
}

function handleUndo() { undo() }
function handleRedo() { redo() }
function handleZoomIn() { editorStore.setZoom(editorStore.zoomLevel + 0.1) }
function handleZoomOut() { editorStore.setZoom(editorStore.zoomLevel - 0.1) }
function handleFitScreen() { editorStore.setZoom(1.0) }
function handleToggleGrid() { editorStore.toggleGrid() }
function handleExportJson() {
  templateStore.exportTemplateJson()
  ElMessage.success('JSON 已导出')
}
</script>

<style scoped>
.editor-view {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: var(--color-bg-primary);
}

.editor-body {
  display: flex;
  flex: 1;
  overflow: hidden;
}

.right-panel {
  width: 320px;
  background: var(--color-bg-secondary);
  border-left: 1px solid var(--color-border-light);
  overflow-y: auto;
  transition: width var(--transition-base);
  position: relative;
  display: flex;
}

.right-panel.collapsed {
  width: 32px;
  overflow: hidden;
}

.right-panel-toggle {
  position: absolute;
  top: 8px;
  left: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: var(--color-text-tertiary);
  z-index: 1;
  border-radius: var(--radius-md);
  transition: all var(--transition-fast);
}

.right-panel-toggle:hover {
  background: var(--color-bg-hover);
  color: var(--color-primary);
}

.right-tabs {
  height: 100%;
  width: 100%;
  padding-left: 32px;
}

.right-tabs :deep(.el-tabs__header) {
  margin: 0;
  padding: 0 var(--spacing-4);
  background: var(--color-bg-secondary);
}

.right-tabs :deep(.el-tabs__nav-wrap::after) {
  display: none;
}

.right-tabs :deep(.el-tabs__content) {
  padding: var(--spacing-4);
  overflow-y: auto;
  height: calc(100% - 44px);
}
</style>
