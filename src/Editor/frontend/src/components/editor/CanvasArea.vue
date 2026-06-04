<template>
  <div
    class="canvas-area"
    ref="containerRef"
    tabindex="0"
    @keydown="handleKeydown"
    @dragover.prevent="onDragOver"
    @drop.prevent="onDrop"
  >
    <div class="canvas-wrapper" ref="wrapperRef">
      <canvas ref="canvasRef"></canvas>
    </div>
    <div class="canvas-zoom-indicator" v-if="currentTemplate">
      {{ Math.round(editorStore.zoomLevel * 100) }}%
    </div>
    <div v-if="!currentTemplate" class="canvas-empty">
      <el-empty :description="$t('properties.noTemplate')" />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { CanvasEngine } from '@/engine'
import { useTemplateStore } from '@/stores/template'
import { useEditorStore } from '@/stores/editor'
import $t from '@/locales/zh-CN'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

const canvasRef = ref(null)
const containerRef = ref(null)
const wrapperRef = ref(null)
const templateStore = useTemplateStore()
const editorStore = useEditorStore()

const currentTemplate = computed(() => templateStore.currentTemplate)
const pageSettingsVersion = computed(() => templateStore.pageSettingsVersion)

let engine = null

function initEngine() {
  const tmpl = currentTemplate.value
  if (!canvasRef.value || !tmpl) return
  destroyEngine()

  try {
    engine = new CanvasEngine(canvasRef.value, tmpl, {
      onElementSelected: (id) => editorStore.selectElement(id),
      onSelectionCleared: () => editorStore.selectElement(null),
      onElementMoving: (id, x, y) => {
        const el = tmpl.elements?.find(e => e.id === id)
        if (el) { el.x = x; el.y = y }
      },
      onElementModified: (id, props) => {
        templateStore.updateElement(id, props)
      }
    })
    engine.init()
    engine.toggleGrid(editorStore.showGrid)
    engine.setZoom(editorStore.zoomLevel)
    templateStore.setEngine(engine)
  } catch (e) {
    console.error('CanvasEngine 初始化失败:', e)
  }
}

function destroyEngine() {
  if (engine) {
    templateStore.setEngine(null)
    engine.destroy()
    engine = null
  }
}

function onDragOver(e) {
  e.dataTransfer.dropEffect = 'copy'
}

function onDrop(e) {
  const elementType = e.dataTransfer.getData('application/x-element-type')
  if (!elementType) return

  const tmpl = currentTemplate.value
  if (!tmpl) return

  const canvasRect = canvasRef.value?.getBoundingClientRect()
  if (!canvasRect) return

  const zoom = editorStore.zoomLevel
  const dropX = (e.clientX - canvasRect.left) / zoom
  const dropY = (e.clientY - canvasRect.top) / zoom

  const xMm = round2((dropX - CANVAS_PADDING) / MM_TO_PX)
  const yMm = round2((dropY - CANVAS_PADDING) / MM_TO_PX)

  const clampedX = round2(Math.max(tmpl.marginLeft, Math.min(xMm, tmpl.pageWidth - tmpl.marginRight)))
  const clampedY = round2(Math.max(tmpl.marginTop, Math.min(yMm, tmpl.pageHeight - tmpl.marginBottom)))

  templateStore.addElementAtPosition(elementType, clampedX, clampedY)
}

function handleKeydown(e) {
  if ((e.ctrlKey || e.metaKey) && e.key === 'z' && !e.shiftKey) {
    e.preventDefault()
    templateStore.undo()
  } else if ((e.ctrlKey || e.metaKey) && (e.key === 'y' || (e.key === 'z' && e.shiftKey))) {
    e.preventDefault()
    templateStore.redo()
  } else if (e.key === 'Delete' || e.key === 'Backspace') {
    if (editorStore.selectedElementId && !e.target.closest('input,textarea,[contenteditable]')) {
      e.preventDefault()
      templateStore.removeElement(editorStore.selectedElementId)
      editorStore.selectElement(null)
    }
  }
}

watch(() => editorStore.showGrid, (show) => engine?.toggleGrid(show))
watch(() => editorStore.zoomLevel, (level) => engine?.setZoom(level))

watch(() => currentTemplate.value?.id, () => {
  if (currentTemplate.value) {
    nextTick(() => initEngine())
  }
})

watch(pageSettingsVersion, () => {
  if (currentTemplate.value) {
    nextTick(() => initEngine())
  }
})

onMounted(() => {
  if (currentTemplate.value) {
    nextTick(() => initEngine())
  }
})

onUnmounted(() => {
  destroyEngine()
})

defineExpose({ engine: () => engine })
</script>

<style scoped>
.canvas-area {
  flex: 1;
  overflow: auto;
  position: relative;
  background: var(--color-surface-dim);
  outline: none;
}
.canvas-wrapper {
  display: flex;
  justify-content: center;
  padding: 20px;
  min-height: 100%;
}
.canvas-empty {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
}
.canvas-zoom-indicator {
  position: absolute;
  bottom: 8px;
  right: 12px;
  background: var(--color-bg-active);
  color: var(--color-text-inverse);
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 4px;
  pointer-events: none;
  z-index: 10;
}
</style>
