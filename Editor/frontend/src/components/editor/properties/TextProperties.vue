<template>
  <div>
    <el-divider content-position="left">文本属性</el-divider>
    <el-form-item label="文本内容">
      <el-input v-model="text" type="textarea" :rows="3" @change="emit('update', 'text', text)" />
    </el-form-item>
    <el-form-item label="富文本">
      <el-switch v-model="isRichText" @change="emit('update', 'isRichText', isRichText)" />
    </el-form-item>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
const props = defineProps({ element: Object })
const emit = defineEmits(['update'])
const text = ref('')
const isRichText = ref(false)
watch(() => props.element, (el) => {
  if (el) { text.value = el.text || ''; isRichText.value = el.isRichText || false }
}, { immediate: true })
</script>
