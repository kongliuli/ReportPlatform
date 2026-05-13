<template>
  <div class="versions-view">
    <el-page-header @back="goBack" :title="$t('toolbar.back')">
      <template #content>
        <span>{{ $t('versions.title') }} - {{ templateName }}</span>
      </template>
    </el-page-header>

    <div class="versions-content">
      <el-timeline v-if="versions.length > 0" v-loading="loading">
        <el-timeline-item
          v-for="version in versions"
          :key="version.id"
          :timestamp="formatTime(version.createTime)"
          placement="top"
          :type="version.id === selectedVersionId ? 'primary' : 'default'"
        >
          <el-card shadow="hover" class="version-card" @click="selectVersion(version)">
            <div class="version-header">
              <el-tag size="small">v{{ version.versionNumber }}</el-tag>
              <span class="version-desc">{{ version.changeDescription || '无描述' }}</span>
            </div>
            <div class="version-meta">
              <span>{{ version.createdBy || '未知' }}</span>
            </div>
            <div class="version-actions">
              <el-button size="small" @click.stop="viewVersion(version)">查看</el-button>
              <el-button size="small" type="warning" @click.stop="confirmRollback(version)">回滚</el-button>
              <el-button size="small" type="primary" @click.stop="startDiff(version)">对比</el-button>
            </div>
          </el-card>
        </el-timeline-item>
      </el-timeline>

      <el-empty v-else :description="$t('versions.title')" />
    </div>

    <VersionDiff
      v-if="showDiff"
      :visible="showDiff"
      :template-id="templateId"
      :version-a="diffVersionA"
      :version-b="diffVersionB"
      @close="showDiff = false"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getVersions, rollbackVersion } from '@/api/version'
import { useTemplateStore } from '@/stores/template'
import VersionDiff from '@/components/preview/VersionDiff.vue'

const route = useRoute()
const router = useRouter()
const templateStore = useTemplateStore()

const templateId = computed(() => route.params.templateId || route.params.id)
const templateName = computed(() => templateStore.currentTemplate?.name || '未知模板')

const versions = ref([])
const loading = ref(false)
const selectedVersionId = ref(null)
const showDiff = ref(false)
const diffVersionA = ref(null)
const diffVersionB = ref(null)

async function loadVersions() {
  loading.value = true
  try {
    const result = await getVersions(templateId.value)
    versions.value = result || []
  } catch (error) {
    ElMessage.error('加载版本历史失败')
  } finally {
    loading.value = false
  }
}

function selectVersion(version) {
  selectedVersionId.value = version.id
}

function viewVersion(version) {
  ElMessage.info(`查看版本 v${version.versionNumber}（功能开发中）`)
}

async function confirmRollback(version) {
  try {
    await ElMessageBox.confirm(
      `确定回滚到版本 v${version.versionNumber}？将创建一个新版本。`,
      '确认回滚',
      { confirmButtonText: '回滚', cancelButtonText: '取消', type: 'warning' }
    )
    await rollbackVersion(templateId.value, version.id)
    ElMessage.success('回滚成功')
    await loadVersions()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('回滚失败')
  }
}

function startDiff(version) {
  if (diffVersionA.value && diffVersionA.value.id !== version.id) {
    diffVersionB.value = version
    showDiff.value = true
    diffVersionA.value = null
  } else {
    diffVersionA.value = version
    ElMessage.info('请选择第二个版本进行对比')
  }
}

function goBack() {
  router.back()
}

function formatTime(time) {
  if (!time) return ''
  return new Date(time).toLocaleString('zh-CN')
}

onMounted(() => {
  loadVersions()
})
</script>

<style scoped>
.versions-view {
  padding: 24px;
}
.versions-content {
  margin-top: 24px;
  max-width: 800px;
}
.version-card {
  cursor: pointer;
  margin-bottom: 8px;
}
.version-header {
  display: flex;
  align-items: center;
  gap: 8px;
}
.version-desc {
  font-size: 14px;
  color: #303133;
}
.version-meta {
  margin-top: 4px;
  font-size: 12px;
  color: #909399;
}
.version-actions {
  margin-top: 8px;
  display: flex;
  gap: 8px;
}
</style>
