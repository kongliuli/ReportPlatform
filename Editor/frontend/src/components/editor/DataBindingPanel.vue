<template>
  <div class="data-binding-panel">
    <el-tabs v-model="activeTab">
      <el-tab-pane :label="$t('databinding.bindPath')" name="paths">
        <DataPathEditor :element="element" @update="emitUpdate" />
      </el-tab-pane>
      <el-tab-pane :label="$t('databinding.transformRules')" name="transform">
        <TransformRuleEditor :element="element" @update="emitUpdate" />
      </el-tab-pane>
      <el-tab-pane :label="$t('databinding.validationRules')" name="validation">
        <ValidationRuleEditor :element="element" @update="emitUpdate" />
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

<script setup>
import $t from '@/locales/zh-CN'
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
