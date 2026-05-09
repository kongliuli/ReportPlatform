import { ref, onMounted, onUnmounted, watch } from 'vue'
import { CanvasEngine } from '@/engine'
import { useTemplateStore } from '@/stores/template'
import { useEditorStore } from '@/stores/editor'

export function useCanvas(canvasRef) {
  const engine = ref(null)
  const templateStore = useTemplateStore()
  const editorStore = useEditorStore()

  function initEngine() {
    if (!canvasRef.value || !templateStore.currentTemplate) return

    destroyEngine()

    engine.value = new CanvasEngine(canvasRef.value, templateStore.currentTemplate, {
      onElementSelected: (id) => {
        editorStore.selectElement(id)
      },
      onSelectionCleared: () => {
        editorStore.selectElement(null)
      },
      onElementMoving: (id, x, y) => {
        templateStore.updateElement(id, { x, y })
      },
      onElementModified: (id, props) => {
        templateStore.updateElement(id, props)
      }
    })

    engine.value.init()
    engine.value.toggleGrid(editorStore.showGrid)
    engine.value.setZoom(editorStore.zoomLevel)
  }

  function destroyEngine() {
    if (engine.value) {
      engine.value.destroy()
      engine.value = null
    }
  }

  watch(() => editorStore.showGrid, (show) => {
    engine.value?.toggleGrid(show)
  })

  watch(() => editorStore.zoomLevel, (level) => {
    engine.value?.setZoom(level)
  })

  watch(() => editorStore.selectedElementId, (id) => {
    if (engine.value?.selectionManager) {
      if (id) {
        engine.value.selectionManager.selectById(id)
      } else {
        engine.value.selectionManager.clearSelection()
      }
    }
  })

  onMounted(() => {
    if (canvasRef.value && templateStore.currentTemplate) {
      initEngine()
    }
  })

  onUnmounted(() => {
    destroyEngine()
  })

  return {
    engine,
    initEngine,
    destroyEngine
  }
}
