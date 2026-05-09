import { IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class PageNumberElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const width = mmToPx(element.width)
    const height = mmToPx(element.height || 20)

    const format = element.format || '{page} / {total}'
    const displayText = format.replace('{page}', '1').replace('{total}', 'N')

    const fabricObj = new IText(displayText, {
      left,
      top,
      width,
      height,
      fontSize: element.fontSize || 12,
      fontFamily: element.fontFamily || 'SimSun',
      fontWeight: element.fontWeight || 'normal',
      fill: element.foregroundColor || '#606266',
      textAlign: element.position || 'center',
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
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
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
