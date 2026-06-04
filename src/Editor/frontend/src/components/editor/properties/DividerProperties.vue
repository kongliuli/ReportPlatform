<template>
  <div class="divider-properties">
    <el-form-item label="线条样式">
      <el-select :model-value="element.dividerStyle" @change="update('dividerStyle', $event)">
        <el-option label="实线" value="solid" />
        <el-option label="虚线" value="dashed" />
        <el-option label="点线" value="dotted" />
      </el-select>
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="粗细">
          <el-input-number :model-value="element.thickness" :min="1" :max="10" @change="update('thickness', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="颜色">
          <el-color-picker :model-value="element.color" @change="update('color', $event)" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="上边距">
          <el-input-number :model-value="element.marginTop" :min="0" :max="50" @change="update('marginTop', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="下边距">
          <el-input-number :model-value="element.marginBottom" :min="0" :max="50" @change="update('marginBottom', $event)" style="width:100%" />
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
