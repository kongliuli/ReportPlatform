import { FabricImage } from 'fabric'
import JsBarcode from 'jsbarcode'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class BarcodeElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const width = mmToPx(element.width)
    const height = mmToPx(element.height || 50)
    const value = element.value || '12345678'
    const format = this._normalizeFormat(element.format || 'CODE128')
    const barWidth = element.barWidth || 2
    const showText = element.showText !== false
    const barcodeHeight = height - (showText ? 16 : 0)

    const tempCanvas = document.createElement('canvas')
    try {
      JsBarcode(tempCanvas, value, {
        format,
        width: barWidth,
        height: barcodeHeight,
        displayValue: showText,
        fontSize: 12,
        margin: 0,
        textMargin: 2
      })
    } catch (_e) {
      JsBarcode(tempCanvas, '12345678', {
        format: 'CODE128',
        width: barWidth,
        height: barcodeHeight,
        displayValue: showText,
        fontSize: 12,
        margin: 0,
        textMargin: 2
      })
    }

    const fabricImage = new FabricImage(tempCanvas, {
      ...this._applyCommonOptions(element, mmToPx),
      scaleX: width / (tempCanvas.width || 1),
      scaleY: height / (tempCanvas.height || 1),
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return fabricImage
  }

  _normalizeFormat(format) {
    const formatMap = {
      CODE128: 'CODE128',
      CODE39: 'CODE39',
      EAN13: 'EAN13',
      EAN8: 'EAN8',
      UPC: 'UPC',
      UPC_E: 'UPC_E',
      ITF: 'ITF14',
      CODABAR: 'codabar'
    }
    return formatMap[format] || 'CODE128'
  }

  update(fabricObj, props) {
    if (props.value !== undefined || props.barWidth !== undefined || props.format !== undefined) {
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
