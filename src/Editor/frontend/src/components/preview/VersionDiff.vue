<template>
  <el-dialog :model-value="visible" title="版本对比" width="70%" @close="emit('close')">
    <div v-loading="loading" class="diff-container">
      <div v-if="diffResult" class="diff-content">
        <div v-if="diffResult.elementDiffs.length > 0" class="diff-section">
          <h4>元素差异</h4>
          <el-table :data="diffResult.elementDiffs" size="small" stripe>
            <el-table-column prop="diffType" label="类型" width="100">
              <template #default="{ row }">
                <el-tag :type="diffTypeTag(row.diffType)" size="small">{{ diffTypeLabel(row.diffType) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="elementType" label="元素类型" width="120" />
            <el-table-column prop="elementId" label="元素ID" width="280" show-overflow-tooltip />
            <el-table-column prop="description" label="说明" />
          </el-table>
        </div>

        <div v-if="diffResult.propertyDiffs.length > 0" class="diff-section">
          <h4>页面属性差异</h4>
          <el-table :data="diffResult.propertyDiffs" size="small" stripe>
            <el-table-column prop="propertyName" label="属性" width="150" />
            <el-table-column prop="oldValue" label="旧值" />
            <el-table-column prop="newValue" label="新值" />
          </el-table>
        </div>

        <el-empty v-if="diffResult.elementDiffs.length === 0 && diffResult.propertyDiffs.length === 0" description="两个版本无差异" />
      </div>
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import apiClient from '@/api/index'

const props = defineProps({
  visible: Boolean,
  templateId: String,
  versionA: Object,
  versionB: Object
})

const emit = defineEmits(['close'])

const loading = ref(false)
const diffResult = ref(null)

watch(() => props.visible, async (val) => {
  if (val && props.versionA && props.versionB) {
    await loadDiff()
  }
})

async function loadDiff() {
  loading.value = true
  try {
    const result = await apiClient.get(`/templates/${props.templateId}/versions/diff`, {
      params: { vidA: props.versionA.id, vidB: props.versionB.id }
    })
    diffResult.value = result.data
  } catch (error) {
    ElMessage.error('加载差异失败')
  } finally {
    loading.value = false
  }
}

function diffTypeTag(type) {
  const map = { Added: 'success', Removed: 'danger', Modified: 'warning' }
  return map[type] || 'info'
}

function diffTypeLabel(type) {
  const map = { Added: '新增', Removed: '删除', Modified: '修改' }
  return map[type] || type
}
</script>

<style scoped>
.diff-container {
  min-height: 200px;
}
.diff-section {
  margin-bottom: 24px;
}
.diff-section h4 {
  margin: 0 0 12px 0;
  color: #303133;
}
</style>
