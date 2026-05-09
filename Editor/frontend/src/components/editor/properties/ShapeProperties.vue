<template>
  <div class="shape-properties">
    <el-form-item label="形状类型">
      <el-select :model-value="element.shapeType" @change="update('shapeType', $event)">
        <el-option label="矩形" value="rectangle" />
        <el-option label="圆形" value="circle" />
        <el-option label="三角形" value="triangle" />
        <el-option label="菱形" value="diamond" />
      </el-select>
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="填充色">
          <el-color-picker :model-value="element.fillColor" @change="update('fillColor', $event)" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="边框色">
          <el-color-picker :model-value="element.strokeColor" @change="update('strokeColor', $event)" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="边框宽度">
          <el-input-number :model-value="element.strokeWidth" :min="0" :max="20" @change="update('strokeWidth', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="边框样式">
          <el-select :model-value="element.strokeStyle" @change="update('strokeStyle', $event)">
            <el-option label="实线" value="solid" />
            <el-option label="虚线" value="dashed" />
            <el-option label="点线" value="dotted" />
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
