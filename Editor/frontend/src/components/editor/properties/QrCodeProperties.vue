<template>
  <div class="qrcode-properties">
    <el-form-item label="二维码内容">
      <el-input :model-value="element.value" @change="update('value', $event)" placeholder="URL或文本" />
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="尺寸">
          <el-input-number :model-value="element.size" :min="50" :max="300" :step="10" @change="update('size', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="容错级别">
          <el-select :model-value="element.errorLevel" @change="update('errorLevel', $event)">
            <el-option label="L (7%)" value="L" />
            <el-option label="M (15%)" value="M" />
            <el-option label="Q (25%)" value="Q" />
            <el-option label="H (30%)" value="H" />
          </el-select>
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="前景色">
          <el-color-picker :model-value="element.foregroundColor" @change="update('foregroundColor', $event)" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="背景色">
          <el-color-picker :model-value="element.backgroundColor" @change="update('backgroundColor', $event)" />
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
