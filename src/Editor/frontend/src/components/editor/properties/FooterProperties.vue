<template>
  <div class="footer-properties">
    <el-form-item label="页脚内容">
      <el-input :model-value="element.content" @change="update('content', $event)" type="textarea" :rows="2" />
    </el-form-item>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="高度">
          <el-input-number :model-value="element.height" :min="20" :max="100" @change="update('height', $event)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="显示页码">
          <el-switch :model-value="element.showPageNumber" @change="update('showPageNumber', $event)" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-form-item label="每页显示">
      <el-switch :model-value="element.showOnAllPages" @change="update('showOnAllPages', $event)" />
    </el-form-item>
  </div>
</template>

<script setup>
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }
</script>
