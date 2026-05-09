import { IText, Rect, Group } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class HyperlinkElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const width = mmToPx(element.width)
    const height = mmToPx(element.height || 20)

    const objects = []

    const displayText = element.text || element.url || '链接'

    const linkText = new IText(displayText, {
      left: 0,
      top: 0,
      fontSize: element.fontSize || 12,
      fontFamily: element.fontFamily || 'SimSun',
      fill: element.linkColor || '#409EFF',
      underline: element.underline ?? true,
      textAlign: element.textAlignment || 'left'
    })
    objects.push(linkText)

    const group = new Group(objects, {
      left,
      top,
      width,
      height,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false,
      hoverCursor: 'pointer'
    })

    group._url = element.url

    return group
  }

  update(fabricObj, props) {
    if (props.text !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text')
      if (text) text.set('text', props.text)
    }
    if (props.url !== undefined) {
      fabricObj._url = props.url
    }
    if (props.linkColor !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text')
      if (text) text.set('fill', props.linkColor)
    }
    if (props.underline !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text')
      if (text) text.set('underline', props.underline)
    }
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
