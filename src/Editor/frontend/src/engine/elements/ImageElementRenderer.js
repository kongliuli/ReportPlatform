import { Rect, Text, Group, FabricImage } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class ImageElementRenderer extends BaseElementRenderer {
  async create(canvas, element, mmToPx) {
    if (element.imageData) {
      const img = await FabricImage.fromURL(
        element.imageData.startsWith('data:') ? element.imageData : `data:image/png;base64,${element.imageData}`
      )
      img.set({
        ...this._applyCommonOptions(element, mmToPx),
        scaleX: mmToPx(element.width) / (img.width || 1),
        scaleY: mmToPx(element.height) / (img.height || 1),
        opacity: element.opacity ?? 1,
        angle: element.rotation || 0
      })
      return img
    }

    const placeholder = new Rect({
      ...this._applyCommonOptions(element, mmToPx),
      width: mmToPx(element.width),
      height: mmToPx(element.height),
      fill: '#f5f5f5',
      stroke: '#d9d9d9',
      strokeWidth: 1,
      strokeDashArray: [5, 5],
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    const label = new Text('图片', {
      left: mmToPx(element.x) + CANVAS_PADDING + mmToPx(element.width) / 2,
      top: mmToPx(element.y) + CANVAS_PADDING + mmToPx(element.height) / 2,
      fontSize: 12,
      fill: '#999',
      originX: 'center',
      originY: 'center',
      selectable: false,
      evented: false
    })

    const { left: _pl, top: _pt, ...groupStyle } = this._applyCommonOptions(element, mmToPx)
    const group = new Group([placeholder, label], groupStyle)

    return group
  }

  update(fabricObj, props) {
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
