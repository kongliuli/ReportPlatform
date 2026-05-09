<template>
  <div class="checkbox-properties">
    <el-form-item label="标签文字">
      <el-input :model-value="element.checkboxLabel" @change="update('checkboxLabel', $event)" placeholder="复选框标签" />
    </el-form-item>
    <el-form-item label="标签位置">
      <el-select :model-value="element.checkboxPosition" @change="update('checkboxPosition', $event)">
        <el-option label="左侧" value="left" />
        <el-option label="右侧" value="right" />
      </el-select>
    </el-form-item>
    <el-form-item label="默认选中">
      <el-switch :model-value="element.checked" @change="update('checked', $event)" />
    </el-form-item>
  </div>
</template>

<script setup>
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }
</script>
