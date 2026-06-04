<template>
  <div>
    <el-divider content-position="left">日期属性</el-divider>
    <el-form-item label="日期值">
      <el-input v-model="value" @change="emit('update','value',value)" />
    </el-form-item>
    <el-form-item label="格式">
      <el-select v-model="format" @change="emit('update','format',format)">
        <el-option label="yyyy-MM-dd" value="yyyy-MM-dd" />
        <el-option label="yyyy/MM/dd" value="yyyy/MM/dd" />
        <el-option label="yyyy年MM月dd日" value="yyyy年MM月dd日" />
        <el-option label="MM/dd/yyyy" value="MM/dd/yyyy" />
      </el-select>
    </el-form-item>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
const props = defineProps({ element: Object })
const emit = defineEmits(['update'])
const value = ref(''), format = ref('yyyy-MM-dd')
watch(() => props.element, (el) => {
  if (el) { value.value = el.value ?? ''; format.value = el.format ?? 'yyyy-MM-dd' }
}, { immediate: true })
</script>
