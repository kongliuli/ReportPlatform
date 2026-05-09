<template>
  <div class="toolbox" :class="{ collapsed: collapsed }">
    <div class="toolbox-header">
      <span v-if="!collapsed" class="toolbox-title">工具箱</span>
      <el-button
        :icon="collapsed ? Expand : Fold"
        text
        size="small"
        @click="$emit('toggle-collapse')"
      />
    </div>
    
    <div v-if="!collapsed" class="toolbox-content">
      <el-collapse v-model="activeCategories" class="category-collapse">
        <el-collapse-item
          v-for="group in groupedElements"
          :key="group.order"
          :name="group.order"
        >
          <template #title>
            <div class="category-title">
              <el-icon><component :is="group.icon" /></el-icon>
              <span>{{ group.label }}</span>
              <span class="category-count">{{ group.elements.length }}</span>
            </div>
          </template>
          
          <div class="toolbox-items">
            <div
              v-for="item in group.elements"
              :key="item.key"
              class="toolbox-item"
              :title="item.label"
              draggable="true"
              @dragstart="onDragStart($event, item.key)"
              @click="$emit('add-element', item.key)"
            >
              <el-icon :size="18"><component :is="item.iconComponent" /></el-icon>
              <span class="toolbox-label">{{ item.label }}</span>
            </div>
          </div>
        </el-collapse-item>
      </el-collapse>
    </div>
    
    <div v-else class="toolbox-items-collapsed">
      <div
        v-for="item in allElements"
        :key="item.key"
        class="toolbox-item-collapsed"
        :title="item.label"
        draggable="true"
        @dragstart="onDragStart($event, item.key)"
        @click="$emit('add-element', item.key)"
      >
        <el-icon :size="16"><component :is="item.iconComponent" /></el-icon>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { 
  Expand, Fold, Document, Picture, Minus, Grid, Calendar, ArrowDown, Histogram,
  CircleCheck, SemiSelect, Select, Tickets, TrendCharts, Files, CopyDocument,
  Top, Bottom, Collection, EditPen, Stamp, Star, Link, DataLine, Edit
} from '@element-plus/icons-vue'
import { ELEMENT_TYPES, ELEMENT_CATEGORIES, getGroupedElements } from '@/models/elements'

defineProps({
  collapsed: { type: Boolean, default: false }
})

defineEmits(['add-element', 'toggle-collapse'])

const activeCategories = ref([1, 2, 3, 4, 5])

const iconMap = { 
  Document, Picture, Minus, Grid, Calendar, ArrowDown, Histogram,
  CircleCheck, SemiSelect, Select, Tickets, TrendCharts, Files, CopyDocument,
  Top, Bottom, Collection, EditPen, Stamp, Star, Link, DataLine, Edit
}

const categoryIconMap = {
  basic: Document,
  input: Edit,
  data: DataLine,
  layout: Grid,
  special: Star
}

const groupedElements = computed(() => {
  return getGroupedElements().map(group => ({
    ...group,
    icon: categoryIconMap[group.elements[0]?.category] || Document,
    elements: group.elements.map(t => ({
      ...t,
      iconComponent: iconMap[t.icon] || Document
    }))
  }))
})

const allElements = computed(() => {
  return ELEMENT_TYPES.map(t => ({
    ...t,
    iconComponent: iconMap[t.icon] || Document
  }))
})

function onDragStart(e, elementType) {
  e.dataTransfer.setData('application/x-element-type', elementType)
  e.dataTransfer.effectAllowed = 'copy'
}
</script>

<style scoped>
.toolbox {
  background: var(--color-bg-secondary);
  border-right: 1px solid var(--color-border-light);
  overflow-y: auto;
  transition: width var(--transition-base);
  width: 220px;
  display: flex;
  flex-direction: column;
}

.toolbox.collapsed {
  width: 52px;
}

.toolbox-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--spacing-3) var(--spacing-3);
  border-bottom: 1px solid var(--color-border-light);
  flex-shrink: 0;
}

.toolbox-title {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.toolbox-content {
  flex: 1;
  overflow-y: auto;
}

.category-collapse {
  border: none;
}

.category-collapse :deep(.el-collapse-item__header) {
  height: 36px;
  line-height: 36px;
  padding: 0 var(--spacing-3);
  background: var(--color-bg-secondary);
  border-bottom: 1px solid var(--color-border-light);
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
}

.category-collapse :deep(.el-collapse-item__wrap) {
  border-bottom: none;
}

.category-collapse :deep(.el-collapse-item__content) {
  padding: 0;
}

.category-title {
  display: flex;
  align-items: center;
  gap: var(--spacing-2);
}

.category-title .el-icon {
  color: var(--color-primary);
}

.category-count {
  margin-left: auto;
  background: var(--color-bg-tertiary);
  padding: 2px 6px;
  border-radius: var(--radius-full);
  font-size: 10px;
  color: var(--color-text-tertiary);
}

.toolbox-items {
  padding: var(--spacing-2);
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
}

.toolbox-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: var(--spacing-3) var(--spacing-2);
  border-radius: var(--radius-md);
  cursor: grab;
  transition: all var(--transition-fast);
  color: var(--color-text-secondary);
  user-select: none;
}

.toolbox-item:hover {
  background: var(--color-primary-bg);
  color: var(--color-primary);
}

.toolbox-item:active {
  cursor: grabbing;
}

.toolbox-label {
  font-size: var(--font-size-xs);
  text-align: center;
}

.toolbox-items-collapsed {
  padding: var(--spacing-2);
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.toolbox-item-collapsed {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-2);
  border-radius: var(--radius-md);
  cursor: grab;
  transition: all var(--transition-fast);
  color: var(--color-text-secondary);
  user-select: none;
}

.toolbox-item-collapsed:hover {
  background: var(--color-primary-bg);
  color: var(--color-primary);
}
</style>
