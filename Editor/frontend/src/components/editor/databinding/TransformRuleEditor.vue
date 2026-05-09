<template>
  <div class="transform-rule-editor">
    <div v-for="(rule, index) in rules" :key="index" class="rule-item">
      <el-row :gutter="8">
        <el-col :span="8">
          <el-select v-model="rule.type" @change="onRuleChange" size="small">
            <el-option v-for="t in ruleTypes" :key="t" :label="t" :value="t" />
          </el-select>
        </el-col>
        <el-col :span="12">
          <el-input v-model="rule.params.expression" placeholder="参数" size="small" @change="onRuleChange" />
        </el-col>
        <el-col :span="4">
          <el-button :icon="Delete" type="danger" size="small" @click="removeRule(index)" text />
        </el-col>
      </el-row>
    </div>
    <el-button size="small" @click="addRule">添加转换规则</el-button>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Delete } from '@element-plus/icons-vue'

const props = defineProps({ element: Object })
const emit = defineEmits(['update'])

const ruleTypes = ['Format', 'Case', 'Math', 'DateFormat', 'Custom']
const rules = ref([])

function addRule() {
  rules.value.push({ type: 'Format', params: { expression: '' } })
  onRuleChange()
}

function removeRule(index) {
  rules.value.splice(index, 1)
  onRuleChange()
}

function onRuleChange() {
  emit('update', '_transformRules', [...rules.value])
}

watch(() => props.element, (el) => {
  if (el) rules.value = el._transformRules || []
}, { immediate: true })
</script>

<style scoped>
.rule-item {
  margin-bottom: 8px;
}
</style>
