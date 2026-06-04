<template>
  <div class="watermark-properties">
    <el-form-item label="水印文字">
      <el-input :model-value="element.text" @change="update('text', $event)" placeholder="水印内容" />
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="字号">
          <el-input-number :model-value="element.fontSize" :min="12" :max="120" @change="update('fontSize', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="旋转角度">
          <el-input-number :model-value="element.angle" :min="-180" :max="180" :step="15" @change="update('angle', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="透明度">
          <el-slider :model-value="element.opacity" :min="0.01" :max="0.5" :step="0.01" @change="update('opacity', $event)" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="平铺">
          <el-switch :model-value="element.repeat" @change="update('repeat', $event)" />
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
