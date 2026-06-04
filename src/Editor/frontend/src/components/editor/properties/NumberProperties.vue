<template>
  <div>
    <el-divider content-position="left">数字属性</el-divider>
    <el-form-item label="数值">
      <el-input-number v-model="value" @change="emit('update','value',value)" style="width:100%" />
    </el-form-item>
    <el-form-item label="单位">
      <el-input v-model="unit" @change="emit('update','unit',unit)" />
    </el-form-item>
    <el-form-item label="格式">
      <el-input v-model="format" placeholder="如 0.00" @change="emit('update','format',format)" />
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="最小值">
          <el-input-number v-model="minValue" @change="emit('update','minValue',minValue)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="最大值">
          <el-input-number v-model="maxValue" @change="emit('update','maxValue',maxValue)" style="width:100%" />
        </el-form-item>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
const props = defineProps({ element: Object })
const emit = defineEmits(['update'])
const value = ref(0), unit = ref(''), format = ref(''), minValue = ref(null), maxValue = ref(null)
watch(() => props.element, (el) => {
  if (el) { value.value = el.value ?? 0; unit.value = el.unit ?? ''; format.value = el.format ?? ''; minValue.value = el.minValue ?? null; maxValue.value = el.maxValue ?? null }
}, { immediate: true })
</script>
