<template>
  <div class="chart-properties">
    <el-form-item label="图表类型">
      <el-select :model-value="element.chartType" @change="update('chartType', $event)">
        <el-option label="柱状图" value="bar" />
        <el-option label="折线图" value="line" />
        <el-option label="饼图" value="pie" />
      </el-select>
    </el-form-item>
    <el-form-item label="数据源路径">
      <el-input :model-value="element.dataSource" @change="update('dataSource', $event)" placeholder="如: data.chartData" />
    </el-form-item>
    <el-form-item label="显示图例">
      <el-switch :model-value="element.legend?.show" @change="updateLegend('show', $event)" />
    </el-form-item>
    <el-form-item label="图例位置">
      <el-select :model-value="element.legend?.position" @change="updateLegend('position', $event)">
        <el-option label="顶部" value="top" />
        <el-option label="底部" value="bottom" />
        <el-option label="左侧" value="left" />
        <el-option label="右侧" value="right" />
      </el-select>
    </el-form-item>
  </div>
</template>

<script setup>
defineProps({ element: { type: Object, required: true } })
const emit = defineEmits(['update'])
function update(key, value) { emit('update', key, value) }
function updateLegend(key, value) {
  const legend = { ...(props.element.legend || {}), [key]: value }
  emit('update', 'legend', legend)
}
</script>
