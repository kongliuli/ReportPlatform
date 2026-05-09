<template>
  <div class="repeat-properties">
    <el-form-item label="数据源路径">
      <el-input :model-value="element.dataSource" @change="update('dataSource', $event)" placeholder="如: items" />
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="最大条数">
          <el-input-number :model-value="element.maxItems" :min="0" :max="100" @change="update('maxItems', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="排列方向">
          <el-select :model-value="element.direction" @change="update('direction', $event)">
            <el-option label="垂直" value="vertical" />
            <el-option label="水平" value="horizontal" />
          </el-select>
        </el-form-item>
      </el-col>
    </el-row>
    <el-form-item label="间距">
      <el-input-number :model-value="element.gap" :min="0" :max="50" @change="update('gap', $event)" style="width:100%" />
    </el-form-item>
  </div>
</template>

<script setup>
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }
</script>
