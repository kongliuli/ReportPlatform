import { Group, Rect, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class FooterElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const width = mmToPx(element.width)
    const height = mmToPx(element.height || 30)

    const objects = []

    const divider = new Rect({
      left: 0,
      top: 0,
      width,
      height: 1,
      fill: '#DCDFE6'
    })
    objects.push(divider)

    const bgRect = new Rect({
      left: 0,
      top: 1,
      width,
      height: height - 1,
      fill: element.backgroundColor || 'transparent',
      stroke: null
    })
    objects.push(bgRect)

    const contentText = new IText(element.content || '页脚内容', {
      left: 4,
      top: 4,
      fontSize: element.fontSize || 12,
      fontFamily: element.fontFamily || 'SimSun',
      fontWeight: element.fontWeight || 'normal',
      fill: element.foregroundColor || '#606266',
      textAlign: element.textAlignment || 'left'
    })
    objects.push(contentText)

    if (element.showPageNumber) {
      const pageText = new IText('{page} / {total}', {
        left: width - 80,
        top: 4,
        fontSize: element.fontSize || 12,
        fontFamily: element.fontFamily || 'SimSun',
        fill: element.foregroundColor || '#606266',
        textAlign: 'right'
      })
      objects.push(pageText)
    }

    const group = new Group(objects, {
      ...this._applyCommonOptions(element, mmToPx),
      width,
      height,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return group
  }

  update(fabricObj, props) {
    if (props.content !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text' && !obj.text.includes('{page}'))
      if (text) text.set('text', props.content)
    }
    if (props.showPageNumber !== undefined) {
      fabricObj.dirty = true
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
