<template>
  <el-dialog :model-value="visible" title="模板预览" width="70%" @close="emit('close')" destroy-on-close>
    <div v-loading="loading" class="preview-container">
      <div class="preview-toolbar">
        <el-radio-group v-model="previewMode" size="small">
          <el-radio-button value="image">图片预览</el-radio-button>
          <el-radio-button value="pdf">打印/PDF</el-radio-button>
        </el-radio-group>
        <div class="preview-actions">
          <el-button size="small" @click="handleDownloadImage" v-if="previewMode === 'image'">
            下载图片
          </el-button>
          <el-button size="small" type="primary" @click="handlePrintPdf" v-if="previewMode === 'pdf'">
            打印/保存PDF
          </el-button>
        </div>
      </div>
      <div class="preview-content" v-if="previewMode === 'image'">
        <img v-if="imageData" :src="imageData" alt="预览" class="preview-image" />
        <el-empty v-else description="暂无预览" />
      </div>
      <div class="preview-content" v-else>
        <div class="pdf-hint">
          <el-icon :size="48"><Printer /></el-icon>
          <p>点击"打印/保存PDF"按钮，在弹出的打印对话框中选择"另存为PDF"即可导出PDF文件</p>
          <el-button type="primary" @click="handlePrintPdf">打印/保存PDF</el-button>
        </div>
      </div>
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, watch } from 'vue'
import { Printer } from '@element-plus/icons-vue'
import { useTemplateStore } from '@/stores/template'
import { captureCanvasAsImage, downloadImage, printCanvasAsPdf } from '@/utils/pdfGenerator'

const props = defineProps({
  visible: Boolean,
  templateId: String
})
const emit = defineEmits(['close'])

const templateStore = useTemplateStore()
const previewMode = ref('image')
const loading = ref(false)
const imageData = ref(null)

watch(() => props.visible, async (val) => {
  if (val) {
    await loadPreview()
  }
})

async function loadPreview() {
  loading.value = true
  try {
    const engine = templateStore.engineRef
    if (engine) {
      imageData.value = captureCanvasAsImage(engine)
    }
  } finally {
    loading.value = false
  }
}

function handleDownloadImage() {
  if (!imageData.value) return
  const name = templateStore.currentTemplate?.name || 'template'
  downloadImage(imageData.value, `${name}.png`)
}

function handlePrintPdf() {
  const engine = templateStore.engineRef
  if (!engine) return
  const name = templateStore.currentTemplate?.name || 'template'
  printCanvasAsPdf(engine, name)
}
</script>

<style scoped>
.preview-container {
  min-height: 400px;
}
.preview-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}
.preview-actions {
  display: flex;
  gap: 8px;
}
.preview-content {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 500px;
  background: #f5f5f5;
  border-radius: 4px;
  overflow: auto;
}
.preview-image {
  max-width: 100%;
  max-height: 70vh;
  box-shadow: 0 2px 12px rgba(0,0,0,0.15);
}
.pdf-hint {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  color: #909399;
  text-align: center;
  padding: 40px;
}
.pdf-hint p {
  max-width: 400px;
  line-height: 1.6;
}
</style>
