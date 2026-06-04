import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { createElementByType } from '@/models/elements'
import { serialize } from '@/utils/serializer'
import { getCachedTemplates, setCachedTemplates, getDraft, setDraft, clearDraft } from '@/utils/templateCache'
import { normalizeTemplate } from '@/utils/templateNormalizer'
import { getTemplates, getTemplate, createTemplateApi, updateTemplateApi, deleteTemplateApi } from '@/api/template'

export const useTemplateStore = defineStore('template', () => {
  const templates = ref([])
  const currentTemplate = ref(null)
  const loading = ref(false)
  const undoStack = ref([])
  const redoStack = ref([])
  const historyLimit = 50
  const engineRef = ref(null)
  const _pageSettingsVersion = ref(0)
  const templateCache = ref(new Map())
  const lastSavedAt = ref(null)
  const hasUnsavedChanges = ref(false)

  const currentElements = computed(() => currentTemplate.value?.elements || [])
  const selectedElement = computed(() => {
    if (!currentTemplate.value || !currentTemplate.value._selectedElementId) return null
    return currentTemplate.value.elements?.find(
      el => el.id === currentTemplate.value._selectedElementId
    ) || null
  })
  const canUndo = computed(() => undoStack.value.length > 0)
  const canRedo = computed(() => redoStack.value.length > 0)
  const lastUndoDescription = computed(() => undoStack.value[undoStack.value.length - 1]?.description || '')
  const lastRedoDescription = computed(() => redoStack.value[redoStack.value.length - 1]?.description || '')
  const pageSettingsVersion = computed(() => _pageSettingsVersion.value)

  function ensureElements() {
    if (!currentTemplate.value) return
    if (!Array.isArray(currentTemplate.value.elements)) {
      currentTemplate.value.elements = []
    }
  }

  function setEngine(engine) {
    engineRef.value = engine
  }

  async function fetchTemplates(params = {}) {
    loading.value = true
    try {
      const result = await getTemplates(params)
      templates.value = result.items || []
      return result
    } finally {
      loading.value = false
    }
  }

  async function fetchTemplate(id) {
    loading.value = true
    try {
      if (templateCache.value.has(id)) {
        const cached = templateCache.value.get(id)
        currentTemplate.value = normalizeTemplate(cached)
        undoStack.value = []
        redoStack.value = []
        return currentTemplate.value
      }

      const result = await getTemplate(id)
      currentTemplate.value = normalizeTemplate(result)
      
      _addToCache(id, result)
      
      undoStack.value = []
      redoStack.value = []
      return currentTemplate.value
    } finally {
      loading.value = false
    }
  }

  async function createTemplate(data) {
    const result = await createTemplateApi(data)
    templates.value.unshift(result)
    return result
  }

  async function updateTemplate(id, data) {
    const result = await updateTemplateApi(id, data)
    const index = templates.value.findIndex(t => t.id === id)
    if (index !== -1) templates.value[index] = result
    if (currentTemplate.value?.id === id) {
      const normalized = normalizeTemplate({ ...result, contentJson: result.contentJson || currentTemplate.value })
      currentTemplate.value = normalized
    }
    return result
  }

  async function deleteTemplate(id) {
    await deleteTemplateApi(id)
    templates.value = templates.value.filter(t => t.id !== id)
    if (currentTemplate.value?.id === id) {
      currentTemplate.value = null
    }
  }

  function addElement(elementOrType, props = {}) {
    if (!currentTemplate.value) return
    ensureElements()

    let element
    if (typeof elementOrType === 'string') {
      element = createElementByType(elementOrType, props)
    } else {
      element = elementOrType
    }

    const engine = engineRef.value
    const tmpl = currentTemplate.value

    const cmd = {
      type: 'addElement',
      executeFn: () => {
        if (!Array.isArray(tmpl.elements)) tmpl.elements = []
        tmpl.elements.push(element)
        if (engine) {
          engine._addFabricObject(element)
          engine.canvas.renderAll()
        }
      },
      undoFn: () => {
        tmpl.elements = tmpl.elements.filter(e => e.id !== element.id)
        if (engine) {
          engine.removeElement(element.id)
        }
      },
      description: `添加${element.getElementType?.() || '元素'}`
    }
    pushHistory(cmd)
    cmd.executeFn()
    return element.id
  }

  function addElementAtPosition(elementOrType, x, y, props = {}) {
    return addElement(elementOrType, { ...props, x, y })
  }

  function updateElement(id, props) {
    if (!currentTemplate.value) return
    const element = currentTemplate.value.elements?.find(e => e.id === id)
    if (!element) return

    const oldProps = {}
    Object.keys(props).forEach(key => { oldProps[key] = element[key] })

    const engine = engineRef.value

    const cmd = {
      type: 'updateElement',
      executeFn: () => {
        Object.assign(element, props)
        if (engine) {
          engine.updateElement(id, props)
        }
      },
      undoFn: () => {
        Object.assign(element, oldProps)
        if (engine) {
          engine.updateElement(id, oldProps)
        }
      },
      description: '修改元素属性'
    }
    pushHistory(cmd)
    cmd.executeFn()
  }

  function removeElement(id) {
    if (!currentTemplate.value) return
    const index = currentTemplate.value.elements?.findIndex(e => e.id === id)
    if (index === -1 || index === undefined) return

    const removedElement = JSON.parse(JSON.stringify(currentTemplate.value.elements[index]))
    const removedType = currentTemplate.value.elements[index].getElementType?.() || 'TextElement'
    const engine = engineRef.value

    const cmd = {
      type: 'removeElement',
      executeFn: () => {
        currentTemplate.value.elements = currentTemplate.value.elements.filter(e => e.id !== id)
        if (engine) engine.removeElement(id)
      },
      undoFn: () => {
        const restored = createElementByType(removedType, removedElement)
        restored.id = id
        currentTemplate.value.elements.splice(index, 0, restored)
        if (engine) {
          engine._addFabricObject(restored)
          engine.canvas.renderAll()
        }
      },
      description: '删除元素'
    }
    pushHistory(cmd)
    cmd.executeFn()
  }

  function moveElement(id, x, y) {
    if (!currentTemplate.value) return
    const element = currentTemplate.value.elements?.find(e => e.id === id)
    if (!element) return

    const oldX = element.x
    const oldY = element.y
    const engine = engineRef.value

    const cmd = {
      type: 'moveElement',
      executeFn: () => {
        element.x = x; element.y = y
        if (engine) engine.updateElement(id, { x, y })
      },
      undoFn: () => {
        element.x = oldX; element.y = oldY
        if (engine) engine.updateElement(id, { x: oldX, y: oldY })
      },
      description: '移动元素'
    }
    pushHistory(cmd)
    cmd.executeFn()
  }

  function reorderElement(id, zIndex) {
    if (!currentTemplate.value) return
    const element = currentTemplate.value.elements?.find(e => e.id === id)
    if (!element) return

    const oldZIndex = element.zIndex
    const engine = engineRef.value

    const cmd = {
      type: 'reorderElement',
      executeFn: () => {
        element.zIndex = zIndex
        if (engine) engine.reorderElement(id, zIndex)
      },
      undoFn: () => {
        element.zIndex = oldZIndex
        if (engine) engine.reorderElement(id, oldZIndex)
      },
      description: '调整层级'
    }
    pushHistory(cmd)
    cmd.executeFn()
  }

  function updatePageSettings(settings) {
    if (!currentTemplate.value) return

    const tmpl = currentTemplate.value
    const oldSettings = {
      pageWidth: tmpl.pageWidth,
      pageHeight: tmpl.pageHeight,
      orientation: tmpl.orientation,
      marginLeft: tmpl.marginLeft,
      marginRight: tmpl.marginRight,
      marginTop: tmpl.marginTop,
      marginBottom: tmpl.marginBottom
    }

    const cmd = {
      type: 'updatePageSettings',
      executeFn: () => {
        Object.assign(tmpl, settings)
        _pageSettingsVersion.value++
      },
      undoFn: () => {
        Object.assign(tmpl, oldSettings)
        _pageSettingsVersion.value++
      },
      description: '修改页面设置'
    }
    pushHistory(cmd)
    cmd.executeFn()
  }

  function pushHistory(command) {
    undoStack.value.push(command)
    if (undoStack.value.length > historyLimit) {
      undoStack.value.shift()
    }
    redoStack.value = []
    _markDirty()
  }

  function undo() {
    if (undoStack.value.length === 0) return
    const cmd = undoStack.value.pop()
    cmd.undoFn()
    redoStack.value.push(cmd)
  }

  function redo() {
    if (redoStack.value.length === 0) return
    const cmd = redoStack.value.pop()
    cmd.executeFn()
    undoStack.value.push(cmd)
  }

  function exportTemplateJson() {
    const tmpl = currentTemplate.value
    if (!tmpl) return

    const exportData = {
      name: tmpl.name,
      type: tmpl.type,
      pageWidth: tmpl.pageWidth,
      pageHeight: tmpl.pageHeight,
      orientation: tmpl.orientation,
      marginLeft: tmpl.marginLeft,
      marginRight: tmpl.marginRight,
      marginTop: tmpl.marginTop,
      marginBottom: tmpl.marginBottom,
      backgroundColor: tmpl.backgroundColor,
      globalFontSize: tmpl.globalFontSize,
      elements: (tmpl.elements || []).map(el => {
        const obj = { ...el }
        return obj
      })
    }

    const jsonStr = JSON.stringify(exportData, null, 2)
    const blob = new Blob([jsonStr], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${tmpl.name || 'template'}.json`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
  }

  function _addToCache(id, data) {
    if (templateCache.value.size >= CACHE_SIZE) {
      const firstKey = templateCache.value.keys().next().value
      templateCache.value.delete(firstKey)
    }
    templateCache.value.set(id, JSON.parse(JSON.stringify(data)))
  }

  function _markDirty() {
    hasUnsavedChanges.value = true
    _autoSaveDraft()
  }

  function _autoSaveDraft() {
    if (!currentTemplate.value?.id) return
    const content = serialize(currentTemplate.value)
    setDraft(currentTemplate.value.id, content)
  }

  function saveDraft() {
    _autoSaveDraft()
  }

  function loadDraft(templateId) {
    const draft = getDraft(templateId)
    if (draft) {
      try {
        const parsed = typeof draft === 'string' ? JSON.parse(draft) : draft
        currentTemplate.value = normalizeTemplate({ id: templateId, contentJson: parsed })
        hasUnsavedChanges.value = true
        return true
      } catch (e) {
        console.warn('Failed to load draft', e)
        return false
      }
    }
    return false
  }

  function clearUnsavedChanges() {
    hasUnsavedChanges.value = false
    lastSavedAt.value = Date.now()
    if (currentTemplate.value?.id) {
      clearDraft(currentTemplate.value.id)
    }
  }

  function cacheTemplateList(list) {
    setCachedTemplates(list)
  }

  function getCachedTemplateList() {
    return getCachedTemplates()
  }

  return {
    templates, currentTemplate, loading, undoStack, redoStack,
    currentElements, selectedElement, canUndo, canRedo,
    lastUndoDescription, lastRedoDescription, pageSettingsVersion,
    lastSavedAt, hasUnsavedChanges,
    setEngine,
    fetchTemplates, fetchTemplate, createTemplate, updateTemplate, deleteTemplate,
    addElement, addElementAtPosition, updateElement, removeElement, moveElement, reorderElement,
    updatePageSettings, exportTemplateJson,
    pushHistory, undo, redo,
    saveDraft, loadDraft, clearUnsavedChanges,
    cacheTemplateList, getCachedTemplateList
  }
})
