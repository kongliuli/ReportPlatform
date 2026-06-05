import { Group, Rect } from 'fabric'
import QRCode from 'qrcode'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class QrCodeElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const size = mmToPx(element.size || 100)
    const value = element.value || 'https://example.com'
    const errorCorrectionLevel = element.errorCorrectionLevel || 'M'

    const qr = QRCode.create(value, { errorCorrectionLevel })
    const moduleCount = qr.modules.size
    const moduleSize = size / moduleCount

    const objects = []

    for (let row = 0; row < moduleCount; row++) {
      for (let col = 0; col < moduleCount; col++) {
        if (qr.modules.data[row * moduleCount + col]) {
          const rect = new Rect({
            left: col * moduleSize,
            top: row * moduleSize,
            width: moduleSize,
            height: moduleSize,
            fill: element.foregroundColor || '#000000'
          })
          objects.push(rect)
        }
      }
    }

    const group = new Group(objects, {
      ...this._applyCommonOptions(element, mmToPx),
      width: size,
      height: size,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return group
  }

  update(fabricObj, props) {
    if (props.value !== undefined || props.size !== undefined) {
      fabricObj.dirty = true
    }
    if (props.foregroundColor !== undefined) {
      fabricObj.getObjects().forEach(obj => {
        obj.set('fill', props.foregroundColor)
      })
    }
    this._applyCommonUpdate(fabricObj, props)
  }

  toModel(fabricObj) {
    return {
      ...this._applyCommonToModel(fabricObj),
      size: round2(fabricObj.getScaledWidth() / MM_TO_PX),
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
