<template>
  <div class="layer-panel">
    <div v-if="elements.length === 0" class="layer-empty">
      <span>暂无元素</span>
    </div>
    <div
      v-for="(el, index) in sortedElements"
      :key="el.id"
      class="layer-item"
      :class="{ active: el.id === selectedId }"
      @click="selectElement(el.id)"
    >
      <el-icon :size="14">
        <component :is="getIcon(el)" />
      </el-icon>
      <span class="layer-name">{{ el.label || el.text || getElementType(el) }}</span>
      <div class="layer-actions">
        <el-icon
          :size="14"
          class="action-icon"
          @click.stop="toggleVisibility(el)"
        >
          <View v-if="el.isVisible !== false" />
          <Hide v-else />
        </el-icon>
        <el-icon :size="14" class="action-icon" @click.stop="moveUp(index)">
          <Top />
        </el-icon>
        <el-icon :size="14" class="action-icon" @click.stop="moveDown(index)">
          <Bottom />
        </el-icon>
        <el-icon :size="14" class="action-icon danger" @click.stop="removeEl(el.id)">
          <Delete />
        </el-icon>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { View, Hide, Top, Bottom, Delete, Document, Picture, Minus, Grid, Calendar, ArrowDown, Histogram } from '@element-plus/icons-vue'
import { useTemplateStore } from '@/stores/template'
import { useEditorStore } from '@/stores/editor'

const templateStore = useTemplateStore()
const editorStore = useEditorStore()

const elements = computed(() => templateStore.currentTemplate?.elements || [])
const selectedId = computed(() => editorStore.selectedElementId)
const sortedElements = computed(() => [...elements.value].sort((a, b) => (b.zIndex || 0) - (a.zIndex || 0)))

const iconMap = { TextElement: Document, ImageElement: Picture, LineElement: Minus, TableElement: Grid, DateElement: Calendar, DropdownElement: ArrowDown, NumberElement: Histogram }

function getIcon(el) { return iconMap[el.getElementType?.()] || Document }
function getElementType(el) { return el.getElementType?.() || 'Element' }

function selectElement(id) { editorStore.selectElement(id) }

function toggleVisibility(el) {
  templateStore.updateElement(el.id, { isVisible: el.isVisible === false })
}

function moveUp(index) {
  const sorted = sortedElements.value
  if (index >= sorted.length - 1) return
  const el = sorted[index]
  const newZ = (sorted[index + 1].zIndex || 0) + 1
  templateStore.updateElement(el.id, { zIndex: newZ })
}

function moveDown(index) {
  if (index <= 0) return
  const sorted = sortedElements.value
  const el = sorted[index]
  const newZ = (sorted[index - 1].zIndex || 0) - 1
  templateStore.updateElement(el.id, { zIndex: newZ })
}

function removeEl(id) {
  templateStore.removeElement(id)
  if (selectedId.value === id) editorStore.selectElement(null)
}
</script>

<style scoped>
.layer-panel {
  padding: 0;
}
.layer-empty {
  text-align: center;
  color: #909399;
  padding: 24px;
  font-size: 13px;
}
.layer-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  cursor: pointer;
  border-bottom: 1px solid #f0f0f0;
  transition: background 0.15s;
}
.layer-item:hover {
  background: #f5f7fa;
}
.layer-item.active {
  background: #ecf5ff;
  border-left: 3px solid #409eff;
}
.layer-name {
  flex: 1;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.layer-actions {
  display: flex;
  gap: 4px;
  opacity: 0;
  transition: opacity 0.15s;
}
.layer-item:hover .layer-actions {
  opacity: 1;
}
.action-icon {
  cursor: pointer;
  color: #909399;
}
.action-icon:hover {
  color: #409eff;
}
.action-icon.danger:hover {
  color: #f56c6c;
}
</style>
