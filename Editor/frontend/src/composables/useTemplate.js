import { computed } from 'vue'
import { useTemplateStore } from '@/stores/template'
import { ElMessage, ElMessageBox } from 'element-plus'

export function useTemplate() {
  const templateStore = useTemplateStore()

  const templates = computed(() => templateStore.templates)
  const currentTemplate = computed(() => templateStore.currentTemplate)
  const loading = computed(() => templateStore.loading)
  const canUndo = computed(() => templateStore.canUndo)
  const canRedo = computed(() => templateStore.canRedo)

  async function loadTemplates(params) {
    try {
      return await templateStore.fetchTemplates(params)
    } catch (error) {
      ElMessage.error(`加载模板列表失败: ${error.message}`)
      throw error
    }
  }

  async function loadTemplate(id) {
    try {
      return await templateStore.fetchTemplate(id)
    } catch (error) {
      ElMessage.error(`加载模板失败: ${error.message}`)
      throw error
    }
  }

  async function saveTemplate() {
    if (!currentTemplate.value) return
    try {
      const result = await templateStore.updateTemplate(currentTemplate.value.id, {
        contentJson: JSON.stringify(currentTemplate.value),
        changeDescription: '保存模板'
      })
      ElMessage.success('保存成功')
      return result
    } catch (error) {
      ElMessage.error(`保存失败: ${error.message}`)
      throw error
    }
  }

  async function removeTemplate(id) {
    try {
      await ElMessageBox.confirm('确定删除该模板？删除后不可恢复。', '确认删除', {
        confirmButtonText: '删除',
        cancelButtonText: '取消',
        type: 'warning'
      })
      await templateStore.deleteTemplate(id)
      ElMessage.success('删除成功')
    } catch (error) {
      if (error !== 'cancel') {
        ElMessage.error(`删除失败: ${error.message}`)
      }
    }
  }

  function undo() {
    templateStore.undo()
  }

  function redo() {
    templateStore.redo()
  }

  return {
    templates,
    currentTemplate,
    loading,
    canUndo,
    canRedo,
    loadTemplates,
    loadTemplate,
    saveTemplate,
    removeTemplate,
    undo,
    redo
  }
}
