<template>
  <div class="data-binding-panel">
    <el-tabs v-model="activeTab">
      <el-tab-pane label="绑定路径" name="paths">
        <DataPathEditor :element="element" @update="emitUpdate" />
      </el-tab-pane>
      <el-tab-pane label="转换规则" name="transform">
        <TransformRuleEditor :element="element" @update="emitUpdate" />
      </el-tab-pane>
      <el-tab-pane label="验证规则" name="validation">
        <ValidationRuleEditor :element="element" @update="emitUpdate" />
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import DataPathEditor from '@/components/editor/databinding/DataPathEditor.vue'
import TransformRuleEditor from '@/components/editor/databinding/TransformRuleEditor.vue'
import ValidationRuleEditor from '@/components/editor/databinding/ValidationRuleEditor.vue'

const props = defineProps({ element: Object })
const emit = defineEmits(['update'])
const activeTab = ref('paths')

function emitUpdate(key, value) {
  emit('update', key, value)
}
</script>

<style scoped>
.data-binding-panel {
  padding: 0;
}
</style>
