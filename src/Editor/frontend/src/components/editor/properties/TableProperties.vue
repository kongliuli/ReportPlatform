<template>
  <div>
    <el-divider content-position="left">表格属性</el-divider>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="行数">
          <el-input-number v-model="rows" :min="1" :max="50" @change="updateTable" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="列数">
          <el-input-number v-model="columns" :min="1" :max="20" @change="updateTable" style="width:100%" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-form-item label="表头">
      <el-switch v-model="hasHeader" @change="emit('update','hasHeader',hasHeader)" />
    </el-form-item>
    <el-form-item label="边框宽度">
      <el-input-number v-model="tableBorder" :min="0" :max="5" @change="emit('update','tableBorder',tableBorder)" style="width:100%" />
    </el-form-item>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
const props = defineProps({ element: Object })
const emit = defineEmits(['update'])
const rows = ref(3), columns = ref(3), hasHeader = ref(true), tableBorder = ref(1)
watch(() => props.element, (el) => {
  if (el) { rows.value = el.rows ?? 3; columns.value = el.columns ?? 3; hasHeader.value = el.hasHeader ?? true; tableBorder.value = el.tableBorder ?? 1 }
}, { immediate: true })
function updateTable() { emit('update', 'rows', rows.value); emit('update', 'columns', columns.value) }
</script>
