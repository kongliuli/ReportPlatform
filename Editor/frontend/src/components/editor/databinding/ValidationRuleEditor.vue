<template>
  <div class="validation-rule-editor">
    <div v-for="(rule, index) in rules" :key="index" class="rule-item">
      <el-row :gutter="8">
        <el-col :span="8">
          <el-select v-model="rule.type" @change="onRuleChange" size="small">
            <el-option v-for="t in ruleTypes" :key="t" :label="t" :value="t" />
          </el-select>
        </el-col>
        <el-col :span="6">
          <el-input v-model="rule.params.min" placeholder="最小" size="small" @change="onRuleChange" v-if="rule.type === 'Range'" />
          <el-input v-model="rule.params.pattern" placeholder="正则" size="small" @change="onRuleChange" v-if="rule.type === 'Regex'" />
          <el-input v-model="rule.params.minLength" placeholder="最小长度" size="small" @change="onRuleChange" v-if="rule.type === 'Length'" />
          <el-input v-model="rule.params.expression" placeholder="表达式" size="small" @change="onRuleChange" v-if="rule.type === 'Custom'" />
        </el-col>
        <el-col :span="6">
          <el-input v-model="rule.params.max" placeholder="最大" size="small" @change="onRuleChange" v-if="rule.type === 'Range'" />
          <el-input v-model="rule.params.maxLength" placeholder="最大长度" size="small" @change="onRuleChange" v-if="rule.type === 'Length'" />
          <el-input v-model="rule.params.message" placeholder="错误提示" size="small" @change="onRuleChange" v-else-if="rule.type !== 'Range' && rule.type !== 'Length'" />
        </el-col>
        <el-col :span="4">
          <el-button :icon="Delete" type="danger" size="small" @click="removeRule(index)" text />
        </el-col>
      </el-row>
    </div>
    <el-button size="small" @click="addRule">添加验证规则</el-button>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Delete } from '@element-plus/icons-vue'

const props = defineProps({ element: Object })
const emit = defineEmits(['update'])

const ruleTypes = ['Required', 'Range', 'Regex', 'Length', 'Custom']
const rules = ref([])

function addRule() {
  rules.value.push({ type: 'Required', params: {} })
  onRuleChange()
}

function removeRule(index) {
  rules.value.splice(index, 1)
  onRuleChange()
}

function onRuleChange() {
  emit('update', '_validationRules', [...rules.value])
}

watch(() => props.element, (el) => {
  if (el) rules.value = el._validationRules || []
}, { immediate: true })
</script>

<style scoped>
.rule-item {
  margin-bottom: 8px;
}
</style>
