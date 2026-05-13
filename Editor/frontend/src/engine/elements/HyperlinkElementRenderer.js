import { IText, Rect, Group } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class HyperlinkElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
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
      ...this._applyCommonOptions(element, mmToPx),
      width,
      height,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
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
