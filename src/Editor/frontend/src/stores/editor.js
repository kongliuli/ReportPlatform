import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useEditorStore = defineStore('editor', () => {
  const selectedElementId = ref(null)
  const zoomLevel = ref(1.0)
  const showGrid = ref(true)
  const snapToGrid = ref(true)
  const activeTool = ref('select')
  const canvasOffset = ref({ x: 0, y: 0 })
  const leftPanelCollapsed = ref(false)
  const rightPanelCollapsed = ref(false)

  const zoomPercentage = computed(() => Math.round(zoomLevel.value * 100))

  function selectElement(id) {
    selectedElementId.value = id
  }

  function setZoom(level) {
    zoomLevel.value = Math.max(0.1, Math.min(5.0, level))
  }

  function toggleGrid() {
    showGrid.value = !showGrid.value
  }

  function setActiveTool(tool) {
    activeTool.value = tool
  }

  function setCanvasOffset(offset) {
    canvasOffset.value = offset
  }

  function toggleLeftPanel() {
    leftPanelCollapsed.value = !leftPanelCollapsed.value
  }

  function toggleRightPanel() {
    rightPanelCollapsed.value = !rightPanelCollapsed.value
  }

  return {
    selectedElementId,
    zoomLevel,
    showGrid,
    snapToGrid,
    activeTool,
    canvasOffset,
    leftPanelCollapsed,
    rightPanelCollapsed,
    zoomPercentage,
    selectElement,
    setZoom,
    toggleGrid,
    setActiveTool,
    setCanvasOffset,
    toggleLeftPanel,
    toggleRightPanel
  }
})
