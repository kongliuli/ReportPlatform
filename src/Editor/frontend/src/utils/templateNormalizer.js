import { ReportTemplateDefinition } from '@/models/template'
import { deserialize } from '@/utils/serializer'

/**
 * 将 API 返回的原始模板数据标准化为 ReportTemplateDefinition 实例。
 * 支持已解析对象、含 contentJson 的对象、和纯属性对象三种格式。
 */
export function normalizeTemplate(raw) {
  if (!raw) return null
  if (raw instanceof ReportTemplateDefinition) return raw

  if (raw.contentJson) {
    try {
      const parsed = typeof raw.contentJson === 'string' ? JSON.parse(raw.contentJson) : raw.contentJson
      const tmpl = new ReportTemplateDefinition({
        ...parsed,
        id: raw.id,
        name: raw.name || parsed.name,
        type: raw.type || parsed.type,
        version: raw.version || parsed.version,
        hospitalId: raw.hospitalId || parsed.hospitalId
      })
      if (parsed.elements && Array.isArray(parsed.elements)) {
        tmpl.elements = parsed.elements.map(el => {
          try { return deserialize(el) } catch { return el }
        })
      }
      return tmpl
    } catch (e) {
      console.warn('解析 contentJson 失败，使用默认模板', e)
    }
  }

  return new ReportTemplateDefinition({
    id: raw.id,
    name: raw.name || '未命名模板',
    type: raw.type || '',
    pageWidth: raw.pageWidth,
    pageHeight: raw.pageHeight,
    orientation: raw.orientation,
    marginLeft: raw.marginLeft,
    marginRight: raw.marginRight,
    marginTop: raw.marginTop,
    marginBottom: raw.marginBottom
  })
}
