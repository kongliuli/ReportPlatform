<template>
  <div>
    <el-divider content-position="left">图片属性</el-divider>
    <el-form-item label="图片路径">
      <el-input v-model="imagePath" @change="emit('update', 'imagePath', imagePath)" />
    </el-form-item>
    <el-form-item label="拉伸模式">
      <el-select v-model="stretch" @change="emit('update', 'stretch', stretch)">
        <el-option label="等比填充" value="Uniform" />
        <el-option label="等比裁剪" value="UniformToFill" />
        <el-option label="拉伸" value="Fill" />
        <el-option label="原始大小" value="None" />
      </el-select>
    </el-form-item>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
const props = defineProps({ element: Object })
const emit = defineEmits(['update'])
const imagePath = ref('')
const stretch = ref('Uniform')
watch(() => props.element, (el) => {
  if (el) { imagePath.value = el.imagePath || ''; stretch.value = el.stretch || 'Uniform' }
}, { immediate: true })
</script>
