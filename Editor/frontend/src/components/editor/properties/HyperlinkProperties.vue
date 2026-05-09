<template>
  <div class="hyperlink-properties">
    <el-form-item label="链接文字">
      <el-input :model-value="element.text" @change="update('text', $event)" placeholder="显示的文字" />
    </el-form-item>
    <el-form-item label="链接地址">
      <el-input :model-value="element.url" @change="update('url', $event)" placeholder="https://..." />
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="打开方式">
          <el-select :model-value="element.target" @change="update('target', $event)">
            <el-option label="新窗口" value="_blank" />
            <el-option label="当前窗口" value="_self" />
          </el-select>
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="下划线">
          <el-switch :model-value="element.underline" @change="update('underline', $event)" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-form-item label="链接颜色">
      <el-color-picker :model-value="element.linkColor" @change="update('linkColor', $event)" />
    </el-form-item>
  </div>
</template>

<script setup>
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }
</script>
