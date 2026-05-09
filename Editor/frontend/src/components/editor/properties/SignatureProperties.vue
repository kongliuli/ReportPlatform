<template>
  <div class="signature-properties">
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="笔触宽度">
          <el-input-number :model-value="element.strokeWidth" :min="1" :max="10" @change="update('strokeWidth', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="笔触颜色">
          <el-color-picker :model-value="element.strokeColor" @change="update('strokeColor', $event)" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-form-item label="背景色">
      <el-color-picker :model-value="element.backgroundColor" @change="update('backgroundColor', $event)" />
    </el-form-item>
  </div>
</template>

<script setup>
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }
</script>
