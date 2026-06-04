import { Group, Rect, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class RepeatElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const width = mmToPx(element.width)
    const height = mmToPx(element.height)

    const objects = []

    const bgRect = new Rect({
      left: 0,
      top: 0,
      width,
      height,
      fill: 'rgba(64, 158, 255, 0.05)',
      stroke: '#409EFF',
      strokeWidth: 1,
      strokeDashArray: [8, 4],
      rx: 4,
      ry: 4
    })
    objects.push(bgRect)

    const placeholderText = new IText('重复区域\n[数据源: ' + (element.dataSource || '未绑定') + ']', {
      left: width / 2,
      top: height / 2,
      fontSize: 12,
      fontFamily: 'SimSun',
      fill: '#909399',
      textAlign: 'center',
      originX: 'center',
      originY: 'center'
    })
    objects.push(placeholderText)

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
    if (props.dataSource !== undefined) {
      const text = fabricObj.getObjects().find(obj => obj.type === 'i-text')
      if (text) {
        text.set('text', '重复区域\n[数据源: ' + (props.dataSource || '未绑定') + ']')
      }
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
