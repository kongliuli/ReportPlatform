import { IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class PageNumberElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const width = mmToPx(element.width)
    const height = mmToPx(element.height || 20)

    const format = element.format || '{page} / {total}'
    const displayText = format.replace('{page}', '1').replace('{total}', 'N')

    const fabricObj = new IText(displayText, {
      ...this._applyCommonOptions(element, mmToPx),
      width,
      height,
      fontSize: element.fontSize || 12,
      fontFamily: element.fontFamily || 'SimSun',
      fontWeight: element.fontWeight || 'normal',
      fill: element.foregroundColor || '#606266',
      textAlign: element.position || 'center',
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return fabricObj
  }

  update(fabricObj, props) {
    if (props.format !== undefined) {
      const displayText = props.format.replace('{page}', '1').replace('{total}', 'N')
      fabricObj.set('text', displayText)
    }
    if (props.fontSize !== undefined) fabricObj.set('fontSize', props.fontSize)
    if (props.fontFamily !== undefined) fabricObj.set('fontFamily', props.fontFamily)
    if (props.foregroundColor !== undefined) fabricObj.set('fill', props.foregroundColor)
    if (props.position !== undefined) fabricObj.set('textAlign', props.position)
    this._applyCommonUpdate(fabricObj, props)
  }

  toModel(fabricObj) {
    return {
      ...this._applyCommonToModel(fabricObj),
      width: round2(fabricObj.getScaledWidth() / MM_TO_PX),
      height: round2(fabricObj.getScaledHeight() / MM_TO_PX),
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
