<template>
  <div class="radio-properties">
    <el-form-item label="排列方向">
      <el-select :model-value="element.orientation" @change="update('orientation', $event)">
        <el-option label="垂直" value="vertical" />
        <el-option label="水平" value="horizontal" />
      </el-select>
    </el-form-item>
    <el-form-item label="选项列表">
      <div class="options-editor">
        <div v-for="(opt, index) in element.options" :key="index" class="option-row">
          <el-input v-model="opt.label" placeholder="显示文本" size="small" @change="updateOptions" />
          <el-input v-model="opt.value" placeholder="值" size="small" @change="updateOptions" />
          <el-button type="danger" :icon="Delete" size="small" @click="removeOption(index)" />
        </div>
        <el-button type="primary" size="small" @click="addOption">添加选项</el-button>
      </div>
    </el-form-item>
    <el-form-item label="默认选中">
      <el-select :model-value="element.selectedValue" @change="update('selectedValue', $event)" clearable>
        <el-option v-for="opt in element.options" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
    </el-form-item>
  </div>
</template>

<script setup>
import { Delete } from '@element-plus/icons-vue'
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }
function updateOptions() { emit('update', 'options', [...props.element.options]) }
function addOption() {
  const options = [...props.element.options, { label: '新选项', value: `opt_${Date.now()}` }]
  emit('update', 'options', options)
}
function removeOption(index) {
  const options = props.element.options.filter((_, i) => i !== index)
  emit('update', 'options', options)
}
</script>

<style scoped>
.options-editor { display: flex; flex-direction: column; gap: 8px; }
.option-row { display: flex; gap: 8px; align-items: center; }
.option-row .el-input { flex: 1; }
</style>
