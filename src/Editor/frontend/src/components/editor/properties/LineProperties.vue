<template>
  <div>
    <el-divider content-position="left">线条属性</el-divider>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="起点X">
          <el-input-number v-model="startX" :step="1" @change="emit('update','startX',startX)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="起点Y">
          <el-input-number v-model="startY" :step="1" @change="emit('update','startY',startY)" style="width:100%" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-row :gutter="8">
      <el-col :span="12">
        <el-form-item label="终点X">
          <el-input-number v-model="endX" :step="1" @change="emit('update','endX',endX)" style="width:100%" />
        </el-form-item>
      </el-col>
      <el-col :span="12">
        <el-form-item label="终点Y">
          <el-input-number v-model="endY" :step="1" @change="emit('update','endY',endY)" style="width:100%" />
        </el-form-item>
      </el-col>
    </el-row>
    <el-form-item label="线条颜色">
      <el-color-picker v-model="lineColor" @change="emit('update','lineColor',lineColor)" />
    </el-form-item>
    <el-form-item label="线条宽度">
      <el-input-number v-model="lineWidth" :min="0.5" :step="0.5" @change="emit('update','lineWidth',lineWidth)" style="width:100%" />
    </el-form-item>
    <el-form-item label="线条样式">
      <el-select v-model="lineStyle" @change="emit('update','lineStyle',lineStyle)">
        <el-option label="实线" value="solid" />
        <el-option label="虚线" value="dashed" />
        <el-option label="点线" value="dotted" />
      </el-select>
    </el-form-item>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
const props = defineProps({ element: Object })
const emit = defineEmits(['update'])
const startX = ref(0), startY = ref(0), endX = ref(100), endY = ref(0)
const lineColor = ref('#000000'), lineWidth = ref(1), lineStyle = ref('solid')
watch(() => props.element, (el) => {
  if (el) {
    startX.value = el.startX ?? 0; startY.value = el.startY ?? 0
    endX.value = el.endX ?? 100; endY.value = el.endY ?? 0
    lineColor.value = el.lineColor ?? '#000000'; lineWidth.value = el.lineWidth ?? 1; lineStyle.value = el.lineStyle ?? 'solid'
  }
}, { immediate: true })
</script>
