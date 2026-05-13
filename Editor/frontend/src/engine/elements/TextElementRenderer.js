import { IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class TextElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const text = element.isRichText && element.richText ? element.richText : (element.text || element.label || '文本')
    const fabricObj = new IText(text, {
      ...this._applyCommonOptions(element, mmToPx),
      width: mmToPx(element.width),
      fontSize: element.fontSize || 12,
      fontFamily: element.fontFamily || 'SimSun',
      fontWeight: element.fontWeight || 'normal',
      fontStyle: element.fontStyle || 'normal',
      fill: element.foregroundColor || '#000000',
      textAlign: element.textAlignment || 'left',
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
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
    this._applyCommonUpdate(fabricObj, props)
  }

  toModel(fabricObj) {
    return {
      ...this._applyCommonToModel(fabricObj),
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
