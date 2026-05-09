import { Rect, Text, Group, FabricImage } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class ImageElementRenderer {
  async create(canvas, element, mmToPx) {
    if (element.imageData) {
      const img = await FabricImage.fromURL(
        element.imageData.startsWith('data:') ? element.imageData : `data:image/png;base64,${element.imageData}`
      )
      img.set({
        left: mmToPx(element.x) + CANVAS_PADDING,
        top: mmToPx(element.y) + CANVAS_PADDING,
        scaleX: mmToPx(element.width) / (img.width || 1),
        scaleY: mmToPx(element.height) / (img.height || 1),
        opacity: element.opacity ?? 1,
        angle: element.rotation || 0,
        borderColor: '#409eff',
        cornerColor: '#409eff',
        cornerSize: 8,
        transparentCorners: false
      })
      return img
    }

    const placeholder = new Rect({
      left: mmToPx(element.x) + CANVAS_PADDING,
      top: mmToPx(element.y) + CANVAS_PADDING,
      width: mmToPx(element.width),
      height: mmToPx(element.height),
      fill: '#f5f5f5',
      stroke: '#d9d9d9',
      strokeWidth: 1,
      strokeDashArray: [5, 5],
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
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

    const group = new Group([placeholder, label], {
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
    })

    return group
  }

  update(fabricObj, props) {
    if (props.opacity !== undefined) fabricObj.set('opacity', props.opacity)
    if (props.rotation !== undefined) fabricObj.set('angle', props.rotation)
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
