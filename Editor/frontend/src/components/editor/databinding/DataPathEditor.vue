<template>
  <div class="data-path-editor">
    <el-form label-position="top" size="small">
      <el-form-item label="主数据路径">
        <el-autocomplete
          v-model="mainPath"
          :fetch-suggestions="querySuggestions"
          placeholder="如 Patient.Name"
          @select="handleSelect"
          @change="emitUpdate"
          style="width:100%"
        />
      </el-form-item>
      <el-form-item label="附加路径">
        <div v-for="(path, key) in additionalPaths" :key="key" style="display:flex;gap:4px;margin-bottom:4px">
          <el-input v-model="additionalPaths[key]" size="small" @change="emitAdditionalPaths" style="flex:1" />
          <el-button size="small" type="danger" :icon="Delete" @click="removePath(key)" text />
        </div>
        <el-button size="small" @click="addPath">添加路径</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Delete } from '@element-plus/icons-vue'

const props = defineProps({ element: Object })
const emit = defineEmits(['update'])

const mainPath = ref('')
const additionalPaths = ref({})

const DATA_PATH_SUGGESTIONS = [
  { value: 'Patient.Name' }, { value: 'Patient.Id' }, { value: 'Patient.Age' },
  { value: 'Patient.Gender' }, { value: 'Patient.BirthDate' }, { value: 'Patient.Phone' },
  { value: 'Report.Title' }, { value: 'Report.Date' }, { value: 'Report.Doctor' },
  { value: 'Report.Department' }, { value: 'Report.Diagnosis' }, { value: 'Report.Conclusion' },
  { value: 'Sample.Id' }, { value: 'Sample.Type' }, { value: 'Sample.CollectionTime' },
  { value: 'Hospital.Name' }, { value: 'Hospital.Code' },
  { value: 'Order.Id' }, { value: 'Order.ItemName' }, { value: 'Order.Result' }, { value: 'Order.Unit' }, { value: 'Order.ReferenceRange' }
]

function querySuggestions(queryString, cb) {
  const results = queryString
    ? DATA_PATH_SUGGESTIONS.filter(s => s.value.toLowerCase().includes(queryString.toLowerCase()))
    : DATA_PATH_SUGGESTIONS
  cb(results)
}

function handleSelect(item) {
  emitUpdate()
}

function emitUpdate() {
  emit('update', 'dataPath', mainPath.value)
}

function addPath() {
  const key = `path_${Date.now()}`
  additionalPaths.value[key] = ''
}

function removePath(key) {
  delete additionalPaths.value[key]
  additionalPaths.value = { ...additionalPaths.value }
  emitAdditionalPaths()
}

function emitAdditionalPaths() {
  emit('update', '_additionalPaths', { ...additionalPaths.value })
}

watch(() => props.element, (el) => {
  if (el) {
    mainPath.value = el.dataPath || ''
    additionalPaths.value = el._additionalPaths || {}
  }
}, { immediate: true })
</script>
