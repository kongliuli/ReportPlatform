import { IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class TextElementRenderer {
  create(canvas, element, mmToPx) {
    const text = element.isRichText && element.richText ? element.richText : (element.text || element.label || '文本')
    const fabricObj = new IText(text, {
      left: mmToPx(element.x) + CANVAS_PADDING,
      top: mmToPx(element.y) + CANVAS_PADDING,
      width: mmToPx(element.width),
      fontSize: element.fontSize || 12,
      fontFamily: element.fontFamily || 'SimSun',
      fontWeight: element.fontWeight || 'normal',
      fontStyle: element.fontStyle || 'normal',
      fill: element.foregroundColor || '#000000',
      textAlign: element.textAlignment || 'left',
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
    })

    if (element.backgroundColor && element.backgroundColor !== 'transparent') {
      fabricObj.set('textBackgroundColor', element.backgroundColor)
    }

    return fabricObj
  }

  update(fabricObj, props) {
    if (props.text !== undefined) fabricObj.set('text', props.text)
    if (props.fontSize !== undefined) fabricObj.set('fontSize', props.fontSize)
    if (props.fontFamily !== undefined) fabricObj.set('fontFamily', props.fontFamily)
    if (props.fontWeight !== undefined) fabricObj.set('fontWeight', props.fontWeight)
    if (props.fontStyle !== undefined) fabricObj.set('fontStyle', props.fontStyle)
    if (props.foregroundColor !== undefined) fabricObj.set('fill', props.foregroundColor)
    if (props.textAlignment !== undefined) fabricObj.set('textAlign', props.textAlignment)
    if (props.opacity !== undefined) fabricObj.set('opacity', props.opacity)
    if (props.rotation !== undefined) fabricObj.set('angle', props.rotation)
    if (props.x !== undefined || props.y !== undefined) {
      fabricObj.set({
        left: (props.x !== undefined ? props.x * MM_TO_PX : fabricObj.left - CANVAS_PADDING) + CANVAS_PADDING,
        top: (props.y !== undefined ? props.y * MM_TO_PX : fabricObj.top - CANVAS_PADDING) + CANVAS_PADDING
      })
    }
  }

  toModel(fabricObj) {
    return {
      x: round2((fabricObj.left - CANVAS_PADDING) / MM_TO_PX),
      y: round2((fabricObj.top - CANVAS_PADDING) / MM_TO_PX),
      width: round2(fabricObj.getScaledWidth() / MM_TO_PX),
      height: round2(fabricObj.getScaledHeight() / MM_TO_PX),
      text: fabricObj.text,
      fontSize: fabricObj.fontSize,
      fontFamily: fabricObj.fontFamily,
      fontWeight: fabricObj.fontWeight,
      fontStyle: fabricObj.fontStyle,
      foregroundColor: fabricObj.fill,
      textAlignment: fabricObj.textAlign,
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
