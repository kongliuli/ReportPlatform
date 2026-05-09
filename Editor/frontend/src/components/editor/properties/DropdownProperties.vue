<template>
  <div>
    <el-divider content-position="left">下拉属性</el-divider>
    <el-form-item label="占位文本">
      <el-input v-model="placeholder" @change="emit('update','placeholder',placeholder)" />
    </el-form-item>
    <el-form-item label="选项列表">
      <div v-for="(opt, i) in options" :key="i" style="display:flex;gap:4px;margin-bottom:4px">
        <el-input v-model="options[i]" size="small" @change="emit('update','options',[...options])" />
        <el-button size="small" type="danger" :icon="Delete" @click="removeOption(i)" text />
      </div>
      <el-button size="small" @click="addOption">添加选项</el-button>
    </el-form-item>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Delete } from '@element-plus/icons-vue'
const props = defineProps({ element: Object })
const emit = defineEmits(['update'])
const placeholder = ref('请选择'), options = ref([])
watch(() => props.element, (el) => {
  if (el) { placeholder.value = el.placeholder ?? '请选择'; options.value = [...(el.options || [])] }
}, { immediate: true })
function addOption() { options.value.push(''); emit('update', 'options', [...options.value]) }
function removeOption(i) { options.value.splice(i, 1); emit('update', 'options', [...options.value]) }
</script>
