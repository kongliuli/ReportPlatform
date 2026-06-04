<template>
  <div class="icon-properties">
    <el-form-item label="图标名称">
      <el-select :model-value="element.iconName" @change="update('iconName', $event)" filterable allow-create>
        <el-option v-for="icon in iconList" :key="icon.value" :label="icon.label" :value="icon.value" />
      </el-select>
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="图标大小">
          <el-input-number :model-value="element.iconSize" :min="12" :max="128" @change="update('iconSize', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="图标颜色">
          <el-color-picker :model-value="element.iconColor" @change="update('iconColor', $event)" />
        </el-form-item>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }

const iconList = [
  { value: 'star', label: '★ 星星' },
  { value: 'heart', label: '♥ 爱心' },
  { value: 'check', label: '✓ 对勾' },
  { value: 'cross', label: '✕ 叉号' },
  { value: 'info', label: 'ℹ 信息' },
  { value: 'warning', label: '⚠ 警告' },
  { value: 'error', label: '✖ 错误' },
  { value: 'success', label: '✔ 成功' },
  { value: 'user', label: '👤 用户' },
  { value: 'document', label: '📄 文档' },
  { value: 'folder', label: '📁 文件夹' },
  { value: 'settings', label: '⚙ 设置' },
  { value: 'search', label: '🔍 搜索' },
  { value: 'mail', label: '✉ 邮件' },
  { value: 'phone', label: '📞 电话' },
  { value: 'location', label: '📍 位置' },
  { value: 'calendar', label: '📅 日历' },
  { value: 'clock', label: '🕐 时钟' },
  { value: 'lock', label: '🔒 锁定' },
  { value: 'unlock', label: '🔓 解锁' }
]
</script>
