<template>
  <div class="container-properties">
    <el-form-item label="布局方式">
      <el-select :model-value="element.layout" @change="update('layout', $event)">
        <el-option label="绝对定位" value="absolute" />
        <el-option label="水平排列" value="horizontal" />
        <el-option label="垂直排列" value="vertical" />
        <el-option label="网格布局" value="grid" />
      </el-select>
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="内边距">
          <el-input-number :model-value="element.padding" :min="0" :max="50" @change="update('padding', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="间距">
          <el-input-number :model-value="element.gap" :min="0" :max="50" @change="update('gap', $event)" style="width:100%" />
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
