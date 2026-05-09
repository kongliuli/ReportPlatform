<template>
  <div class="barcode-properties">
    <el-form-item label="条码值">
      <el-input :model-value="element.value" @change="update('value', $event)" placeholder="条码内容" />
    </el-form-item>
    <el-form-item label="条码格式">
      <el-select :model-value="element.barcodeFormat" @change="update('barcodeFormat', $event)">
        <el-option label="CODE128" value="CODE128" />
        <el-option label="CODE39" value="CODE39" />
        <el-option label="EAN13" value="EAN13" />
      </el-select>
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="条宽">
          <el-input-number :model-value="element.barWidth" :min="1" :max="5" @change="update('barWidth', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="显示文字">
          <el-switch :model-value="element.showText" @change="update('showText', $event)" />
        </el-form-item>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }
</script>
