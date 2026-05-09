import { getElementClassByType } from '@/models/elements'
import { ReportTemplateDefinition } from '@/models/template'

const EXCLUDED_ELEMENT_FIELDS = new Set([
  'rotation',
  'shadow',
  'labelWidth',
  'isDataBound',
  'richText',
  'isRichText',
  'minValue',
  'maxValue',
  'minDate',
  'maxDate'
])

const EXCLUDED_TEMPLATE_FIELDS = new Set([
  'id',
  'hospitalId',
  'isDefault',
  'isForceUpdate',
  'createTime',
  'updateTime',
  'enableGlobalFontSize',
  'dataBindings'
])

export function serialize(obj) {
  const prepared = prepareForSerialization(obj)
  return JSON.stringify(prepared, null, 2)
}

function isTemplateObject(obj) {
  return obj instanceof ReportTemplateDefinition || (obj.name !== undefined && obj.elements !== undefined)
}

function prepareForSerialization(obj) {
  if (obj === null || obj === undefined) return obj
  if (typeof obj !== 'object') return obj

  if (Array.isArray(obj)) {
    return obj.map(item => prepareForSerialization(item))
  }

  const isTemplate = isTemplateObject(obj)

  const result = {}

  if (obj.$type) {
    result.$type = obj.$type
  }

  for (const [key, value] of Object.entries(obj)) {
    if (key.startsWith('_') || typeof value === 'function') continue
    if (EXCLUDED_ELEMENT_FIELDS.has(key)) continue
    if (isTemplate && EXCLUDED_TEMPLATE_FIELDS.has(key)) continue
    result[key] = prepareForSerialization(value)
  }

  return result
}

export function deserialize(json) {
  if (typeof json === 'string') {
    json = JSON.parse(json)
  }
  return reviveObject(json)
}

function reviveObject(obj) {
  if (obj === null || obj === undefined) return obj
  if (typeof obj !== 'object') return obj

  if (Array.isArray(obj)) {
    return obj.map(item => reviveObject(item))
  }

  if (obj.$type) {
    const Cls = getElementClassByType(obj.$type)
    if (Cls) {
      const cleaned = { ...obj }
      return new Cls(cleaned)
    }
  }

  const result = {}
  for (const [key, value] of Object.entries(obj)) {
    result[key] = reviveObject(value)
  }
  return result
}

export function deserializeTemplate(json) {
  if (typeof json === 'string') {
    json = JSON.parse(json)
  }

  const template = new ReportTemplateDefinition(json)

  if (json.elements && Array.isArray(json.elements)) {
    template.elements = json.elements.map(el => reviveObject(el))
  }

  return template
}
