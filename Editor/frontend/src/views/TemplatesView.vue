<template>
  <div class="templates-view">
    <div class="page-header animate-slide-up">
      <div class="header-content">
        <h1 class="page-title">模板管理</h1>
        <p class="page-description">创建、编辑和管理您的报告模板</p>
      </div>
      <el-button type="primary" size="large" :icon="Plus" @click="handleCreate">
        新建模板
      </el-button>
    </div>

    <div class="filter-card animate-slide-up" style="animation-delay: 50ms">
      <div class="filter-row">
        <div class="filter-item filter-item--search">
          <el-input
            v-model="filter.name"
            placeholder="搜索模板名称..."
            :prefix-icon="Search"
            clearable
            @clear="loadData"
            @keyup.enter="loadData"
          />
        </div>
        <div class="filter-item">
          <el-select
            v-model="filter.type"
            placeholder="全部类型"
            clearable
            @change="loadData"
          >
            <el-option v-for="t in templateTypes" :key="t" :label="t" :value="t" />
          </el-select>
        </div>
        <div class="filter-item">
          <el-select
            v-model="filter.isPublished"
            placeholder="全部状态"
            clearable
            @change="loadData"
          >
            <el-option label="已发布" :value="true" />
            <el-option label="草稿" :value="false" />
          </el-select>
        </div>
        <el-button type="primary" :icon="Search" @click="loadData">搜索</el-button>
      </div>
    </div>

    <div class="table-card animate-slide-up" style="animation-delay: 100ms">
      <el-table
        :data="templates"
        v-loading="loading"
        class="templates-table"
        @row-click="handleRowClick"
      >
        <el-table-column prop="name" label="模板名称" min-width="200">
          <template #default="{ row }">
            <div class="template-name-cell">
              <div class="template-icon">
                <el-icon><Document /></el-icon>
              </div>
              <div class="template-info">
                <span class="template-name">{{ row.name }}</span>
                <span class="template-type-badge">{{ row.type }}</span>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="version" label="版本" width="100" align="center">
          <template #default="{ row }">
            <span class="version-badge">v{{ row.version }}</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="120" align="center">
          <template #default="{ row }">
            <el-tag :type="row.isPublished ? 'success' : 'info'" size="small">
              {{ row.isPublished ? '已发布' : '草稿' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="updateTime" label="更新时间" width="180">
          <template #default="{ row }">
            <div class="time-cell">
              <el-icon><Clock /></el-icon>
              <span>{{ formatTime(row.updateTime) }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="280" fixed="right">
          <template #default="{ row }">
            <div class="action-buttons">
              <el-button size="small" type="primary" :icon="Edit" @click.stop="handleEdit(row)">
                编辑
              </el-button>
              <el-button size="small" :icon="Clock" @click.stop="handleVersions(row)">
                版本
              </el-button>
              <el-dropdown trigger="click" @command="(cmd) => handleAction(cmd, row)" @click.stop>
                <el-button size="small" :icon="MoreFilled">
                  <el-icon class="el-icon--right"><ArrowDown /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item :command="'toggle'" :icon="row.isPublished ? Remove : CircleCheck">
                      {{ row.isPublished ? '取消发布' : '发布' }}
                    </el-dropdown-item>
                    <el-dropdown-item :command="'delete'" :icon="Delete" divided>
                      <span class="danger-text">删除</span>
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-wrapper">
        <el-pagination
          v-model:current-page="filter.page"
          v-model:page-size="filter.pageSize"
          :total="totalCount"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next"
          @size-change="loadData"
          @current-change="loadData"
        />
      </div>
    </div>

    <el-dialog
      v-model="showCreateDialog"
      title="新建模板"
      width="520px"
      :close-on-click-modal="false"
      class="create-dialog"
    >
      <el-form
        ref="createFormRef"
        :model="createForm"
        :rules="createRules"
        label-position="top"
        class="create-form"
      >
        <el-form-item label="模板名称" prop="name">
          <el-input
            v-model="createForm.name"
            placeholder="请输入模板名称"
            maxlength="100"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="模板类型" prop="type">
          <el-select v-model="createForm.type" placeholder="请选择类型" style="width: 100%">
            <el-option v-for="t in templateTypes" :key="t" :label="t" :value="t" />
          </el-select>
        </el-form-item>
        <el-form-item label="医院ID">
          <el-input v-model="createForm.hospitalId" placeholder="可选，用于多租户场景" />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="showCreateDialog = false">取消</el-button>
          <el-button type="primary" @click="submitCreate" :loading="creating">
            创建并编辑
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Plus,
  Search,
  Document,
  Clock,
  Edit,
  MoreFilled,
  ArrowDown,
  CircleCheck,
  Remove,
  Delete
} from '@element-plus/icons-vue'
import { useTemplateStore } from '@/stores/template'
import { useAuthStore } from '@/stores/auth'
import { ReportTemplateDefinition } from '@/models/template'
import { serialize } from '@/utils/serializer'

const router = useRouter()
const templateStore = useTemplateStore()
const authStore = useAuthStore()

const templateTypes = ['检验报告', '体检报告', '病理报告', '影像报告', '手术记录', '护理记录', '其他']

const templates = ref([])
const loading = ref(false)
const totalCount = ref(0)
const filter = reactive({ name: '', type: '', isPublished: null, page: 1, pageSize: 20 })

const showCreateDialog = ref(false)
const creating = ref(false)
const createFormRef = ref()
const createForm = reactive({ name: '', type: '', hospitalId: '' })
const createRules = {
  name: [{ required: true, message: '请输入模板名称', trigger: 'blur' }],
  type: [{ required: true, message: '请选择模板类型', trigger: 'change' }]
}

async function loadData() {
  loading.value = true
  try {
    const result = await templateStore.fetchTemplates(filter)
    templates.value = result.items || []
    totalCount.value = result.totalCount || 0
  } catch (e) {
    ElMessage.error('加载模板列表失败')
  } finally {
    loading.value = false
  }
}

function handleRowClick(row) {
  router.push(`/editor/${row.id}`)
}

function handleEdit(row) {
  router.push(`/editor/${row.id}`)
}

function handleVersions(row) {
  router.push(`/templates/${row.id}/versions`)
}

async function handleAction(command, row) {
  if (command === 'toggle') {
    await handleTogglePublish(row)
  } else if (command === 'delete') {
    await handleDelete(row)
  }
}

async function handleTogglePublish(row) {
  try {
    await templateStore.updateTemplate(row.id, { isPublished: !row.isPublished })
    ElMessage.success(row.isPublished ? '已取消发布' : '已发布')
    await loadData()
  } catch (e) {
    ElMessage.error('操作失败')
  }
}

async function handleDelete(row) {
  try {
    await ElMessageBox.confirm(
      `确定删除模板「${row.name}」吗？删除后无法恢复。`,
      '确认删除',
      {
        confirmButtonText: '删除',
        cancelButtonText: '取消',
        type: 'warning',
        confirmButtonClass: 'el-button--danger'
      }
    )
    await templateStore.deleteTemplate(row.id)
    ElMessage.success('删除成功')
    await loadData()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
}

function handleCreate() {
  createForm.name = ''
  createForm.type = ''
  createForm.hospitalId = ''
  showCreateDialog.value = true
}

async function submitCreate() {
  const valid = await createFormRef.value?.validate().catch(() => false)
  if (!valid) return

  creating.value = true
  try {
    const template = new ReportTemplateDefinition({
      name: createForm.name,
      type: createForm.type,
      hospitalId: createForm.hospitalId || authStore.user?.hospitalId
    })
    const result = await templateStore.createTemplate({
      name: createForm.name,
      type: createForm.type,
      hospitalId: createForm.hospitalId || authStore.user?.hospitalId,
      contentJson: serialize(template),
      createdBy: authStore.user?.username
    })
    showCreateDialog.value = false
    ElMessage.success('创建成功')
    router.push(`/editor/${result.id}`)
  } catch (e) {
    ElMessage.error('创建失败')
  } finally {
    creating.value = false
  }
}

function formatTime(time) {
  if (!time) return ''
  return new Date(time).toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(() => loadData())
</script>

<style scoped>
.templates-view {
  padding: var(--spacing-8);
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--spacing-6);
}

.page-title {
  font-size: var(--font-size-3xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  margin: 0 0 var(--spacing-2) 0;
  letter-spacing: var(--letter-spacing-tight);
}

.page-description {
  font-size: var(--font-size-base);
  color: var(--color-text-tertiary);
  margin: 0;
}

.filter-card {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-xl);
  padding: var(--spacing-5);
  margin-bottom: var(--spacing-5);
}

.filter-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-4);
}

.filter-item {
  flex-shrink: 0;
}

.filter-item--search {
  flex: 1;
  min-width: 280px;
}

.filter-item--search :deep(.el-input__wrapper) {
  background: var(--color-bg-primary);
}

.filter-item :deep(.el-select) {
  width: 160px;
}

.table-card {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-xl);
  overflow: hidden;
}

.templates-table {
  width: 100%;
}

.templates-table :deep(.el-table__row) {
  cursor: pointer;
  transition: background var(--transition-fast);
}

.templates-table :deep(.el-table__row:hover) {
  background: var(--color-bg-hover);
}

.template-name-cell {
  display: flex;
  align-items: center;
  gap: var(--spacing-3);
}

.template-icon {
  width: 40px;
  height: 40px;
  background: var(--color-primary-bg);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  flex-shrink: 0;
}

.template-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.template-name {
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
}

.template-type-badge {
  font-size: var(--font-size-xs);
  color: var(--color-text-tertiary);
  padding: 2px 8px;
  background: var(--color-bg-tertiary);
  border-radius: var(--radius-sm);
  width: fit-content;
}

.version-badge {
  display: inline-block;
  padding: 4px 10px;
  background: var(--color-bg-tertiary);
  border-radius: var(--radius-full);
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  font-family: var(--font-family-mono);
}

.time-cell {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

.time-cell .el-icon {
  color: var(--color-text-tertiary);
}

.action-buttons {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.danger-text {
  color: var(--color-error);
}

.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
  padding: var(--spacing-5);
  border-top: 1px solid var(--color-border-light);
}

.create-form {
  padding: var(--spacing-2) 0;
}

.create-form :deep(.el-form-item__label) {
  font-weight: var(--font-weight-medium);
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-3);
}

@media (max-width: 900px) {
  .templates-view {
    padding: var(--spacing-4);
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--spacing-4);
  }

  .filter-row {
    flex-wrap: wrap;
  }

  .filter-item--search {
    min-width: 100%;
  }
}
</style>
