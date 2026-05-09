<template>
  <div class="pagenumber-properties">
    <el-form-item label="页码格式">
      <el-select :model-value="element.format" @change="update('format', $event)">
        <el-option label="1 / N" value="{page} / {total}" />
        <el-option label="第 1 页" value="第 {page} 页" />
        <el-option label="Page 1 of N" value="Page {page} of {total}" />
        <el-option label="1" value="{page}" />
      </el-select>
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="起始页码">
          <el-input-number :model-value="element.startFrom" :min="1" :max="999" @change="update('startFrom', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="对齐方式">
          <el-select :model-value="element.position" @change="update('position', $event)">
            <el-option label="左对齐" value="left" />
            <el-option label="居中" value="center" />
            <el-option label="右对齐" value="right" />
          </el-select>
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
