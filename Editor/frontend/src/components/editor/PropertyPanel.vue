<template>
  <div class="property-panel">
    <template v-if="selectedElement">
      <el-form label-position="top" size="small">
        <el-divider content-position="left">基础属性</el-divider>
        <el-form-item label="标签">
          <el-input v-model="form.label" @change="updateProp('label', form.label)" />
        </el-form-item>
        <el-row :gutter="8">
          <el-col :span="12">
            <el-form-item label="X (mm)">
              <el-input-number v-model="form.x" :step="0.5" :precision="2" @change="updateProp('x', form.x)" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="Y (mm)">
              <el-input-number v-model="form.y" :step="0.5" :precision="2" @change="updateProp('y', form.y)" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="8">
          <el-col :span="12">
            <el-form-item label="宽 (mm)">
              <el-input-number v-model="form.width" :min="1" :step="0.5" :precision="2" @change="updateProp('width', form.width)" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="高 (mm)">
              <el-input-number v-model="form.height" :min="1" :step="0.5" :precision="2" @change="updateProp('height', form.height)" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider content-position="left">外观</el-divider>
        <el-row :gutter="8">
          <el-col :span="12">
            <el-form-item label="前景色">
              <el-color-picker v-model="form.foregroundColor" @change="updateProp('foregroundColor', form.foregroundColor)" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="背景色">
              <el-color-picker v-model="form.backgroundColor" @change="updateProp('backgroundColor', form.backgroundColor)" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="透明度">
          <el-slider v-model="form.opacity" :min="0" :max="1" :step="0.1" @change="updateProp('opacity', form.opacity)" />
        </el-form-item>
        <el-row :gutter="8">
          <el-col :span="12">
            <el-form-item label="边框色">
              <el-color-picker v-model="form.borderColor" @change="updateProp('borderColor', form.borderColor)" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="边框宽">
              <el-input-number v-model="form.borderWidth" :min="0" :step="1" @change="updateProp('borderWidth', form.borderWidth)" style="width:100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="圆角">
          <el-input-number v-model="form.cornerRadius" :min="0" :step="1" @change="updateProp('cornerRadius', form.cornerRadius)" style="width:100%" />
        </el-form-item>

        <el-divider content-position="left">字体</el-divider>
        <el-form-item label="字体">
          <el-select v-model="form.fontFamily" @change="updateProp('fontFamily', form.fontFamily)">
            <el-option v-for="f in fontFamilies" :key="f" :label="f" :value="f" />
          </el-select>
        </el-form-item>
        <el-row :gutter="8">
          <el-col :span="8">
            <el-form-item label="大小">
              <el-input-number v-model="form.fontSize" :min="6" :max="72" @change="updateProp('fontSize', form.fontSize)" style="width:100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="粗细">
              <el-select v-model="form.fontWeight" @change="updateProp('fontWeight', form.fontWeight)">
                <el-option label="正常" value="normal" />
                <el-option label="粗体" value="bold" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="对齐">
              <el-select v-model="form.textAlignment" @change="updateProp('textAlignment', form.textAlignment)">
                <el-option label="左" value="left" />
                <el-option label="中" value="center" />
                <el-option label="右" value="right" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <TextProperties v-if="elementType === 'TextElement'" :element="selectedElement" @update="updateProp" />
        <ImageProperties v-else-if="elementType === 'ImageElement'" :element="selectedElement" @update="updateProp" />
        <LineProperties v-else-if="elementType === 'LineElement'" :element="selectedElement" @update="updateProp" />
        <ShapeProperties v-else-if="elementType === 'ShapeElement'" :element="selectedElement" @update="updateProp" />
        <DividerProperties v-else-if="elementType === 'DividerElement'" :element="selectedElement" @update="updateProp" />
        <CheckboxProperties v-else-if="elementType === 'CheckboxElement'" :element="selectedElement" @update="updateProp" />
        <RadioProperties v-else-if="elementType === 'RadioElement'" :element="selectedElement" @update="updateProp" />
        <DropdownProperties v-else-if="elementType === 'DropdownElement'" :element="selectedElement" @update="updateProp" />
        <NumberProperties v-else-if="elementType === 'NumberElement'" :element="selectedElement" @update="updateProp" />
        <DateProperties v-else-if="elementType === 'DateElement'" :element="selectedElement" @update="updateProp" />
        <TableProperties v-else-if="elementType === 'TableElement'" :element="selectedElement" @update="updateProp" />
        <BarcodeProperties v-else-if="elementType === 'BarcodeElement'" :element="selectedElement" @update="updateProp" />
        <QrCodeProperties v-else-if="elementType === 'QrCodeElement'" :element="selectedElement" @update="updateProp" />
        <ChartProperties v-else-if="elementType === 'ChartElement'" :element="selectedElement" @update="updateProp" />
        <ContainerProperties v-else-if="elementType === 'ContainerElement'" :element="selectedElement" @update="updateProp" />
        <RepeatProperties v-else-if="elementType === 'RepeatElement'" :element="selectedElement" @update="updateProp" />
        <HeaderProperties v-else-if="elementType === 'HeaderElement'" :element="selectedElement" @update="updateProp" />
        <FooterProperties v-else-if="elementType === 'FooterElement'" :element="selectedElement" @update="updateProp" />
        <PageNumberProperties v-else-if="elementType === 'PageNumberElement'" :element="selectedElement" @update="updateProp" />
        <SignatureProperties v-else-if="elementType === 'SignatureElement'" :element="selectedElement" @update="updateProp" />
        <WatermarkProperties v-else-if="elementType === 'WatermarkElement'" :element="selectedElement" @update="updateProp" />
        <IconProperties v-else-if="elementType === 'IconElement'" :element="selectedElement" @update="updateProp" />
        <HyperlinkProperties v-else-if="elementType === 'HyperlinkElement'" :element="selectedElement" @update="updateProp" />

        <el-divider content-position="left">数据绑定</el-divider>
        <DataBindingPanel :element="selectedElement" @update="updateProp" />
      </el-form>
    </template>
    <el-empty v-else description="选择元素以编辑属性" :image-size="80" />
  </div>
</template>

<script setup>
import { computed, reactive, watch } from 'vue'
import { useTemplateStore } from '@/stores/template'
import { useEditorStore } from '@/stores/editor'
import TextProperties from './properties/TextProperties.vue'
import ImageProperties from './properties/ImageProperties.vue'
import LineProperties from './properties/LineProperties.vue'
import ShapeProperties from './properties/ShapeProperties.vue'
import DividerProperties from './properties/DividerProperties.vue'
import CheckboxProperties from './properties/CheckboxProperties.vue'
import RadioProperties from './properties/RadioProperties.vue'
import DropdownProperties from './properties/DropdownProperties.vue'
import NumberProperties from './properties/NumberProperties.vue'
import DateProperties from './properties/DateProperties.vue'
import TableProperties from './properties/TableProperties.vue'
import BarcodeProperties from './properties/BarcodeProperties.vue'
import QrCodeProperties from './properties/QrCodeProperties.vue'
import ChartProperties from './properties/ChartProperties.vue'
import ContainerProperties from './properties/ContainerProperties.vue'
import RepeatProperties from './properties/RepeatProperties.vue'
import HeaderProperties from './properties/HeaderProperties.vue'
import FooterProperties from './properties/FooterProperties.vue'
import PageNumberProperties from './properties/PageNumberProperties.vue'
import SignatureProperties from './properties/SignatureProperties.vue'
import WatermarkProperties from './properties/WatermarkProperties.vue'
import IconProperties from './properties/IconProperties.vue'
import HyperlinkProperties from './properties/HyperlinkProperties.vue'
import DataBindingPanel from './DataBindingPanel.vue'

const templateStore = useTemplateStore()
const editorStore = useEditorStore()

const selectedElement = computed(() => {
  const id = editorStore.selectedElementId
  if (!id || !templateStore.currentTemplate) return null
  return templateStore.currentTemplate.elements?.find(e => e.id === id) || null
})

const elementType = computed(() => selectedElement.value?.getElementType?.() || '')

const fontFamilies = ['SimSun', 'SimHei', 'Microsoft YaHei', 'KaiTi', 'FangSong', 'Arial', 'Times New Roman']

const form = reactive({
  label: '', x: 0, y: 0, width: 100, height: 30,
  foregroundColor: '#000000', backgroundColor: 'transparent',
  opacity: 1, borderColor: '#000000', borderWidth: 0, cornerRadius: 0,
  fontFamily: 'SimSun', fontSize: 12, fontWeight: 'normal', textAlignment: 'left',
  dataPath: '', formatString: ''
})

watch(selectedElement, (el) => {
  if (el) {
    Object.keys(form).forEach(key => {
      if (el[key] !== undefined) form[key] = el[key]
    })
  }
}, { immediate: true })

function updateProp(key, value) {
  if (!selectedElement.value) return
  templateStore.updateElement(selectedElement.value.id, { [key]: value })
}
</script>

<style scoped>
.property-panel {
  padding: 0;
}
</style>
