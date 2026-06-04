import { Group, Rect } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class QrCodeElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const size = mmToPx(element.size || 100)
    const value = element.value || 'https://example.com'
    const moduleCount = 25
    const moduleSize = size / moduleCount

    const objects = []
    const qrMatrix = this._generateQRMatrix(value, moduleCount)

    for (let row = 0; row < moduleCount; row++) {
      for (let col = 0; col < moduleCount; col++) {
        if (qrMatrix[row][col]) {
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

  _generateQRMatrix(value, moduleCount) {
    const matrix = []
    for (let i = 0; i < moduleCount; i++) {
      matrix[i] = []
      for (let j = 0; j < moduleCount; j++) {
        matrix[i][j] = false
      }
    }

    this._addFinderPattern(matrix, 0, 0)
    this._addFinderPattern(matrix, moduleCount - 7, 0)
    this._addFinderPattern(matrix, 0, moduleCount - 7)

    const seed = this._hashCode(value)
    for (let i = 0; i < moduleCount; i++) {
      for (let j = 0; j < moduleCount; j++) {
        if (!matrix[i][j] && !this._isInFinderPattern(i, j, moduleCount)) {
          matrix[i][j] = this._pseudoRandom(seed, i * moduleCount + j) > 0.5
        }
      }
    }

    return matrix
  }

  _addFinderPattern(matrix, startRow, startCol) {
    for (let i = 0; i < 7; i++) {
      for (let j = 0; j < 7; j++) {
        if (i === 0 || i === 6 || j === 0 || j === 6 ||
            (i >= 2 && i <= 4 && j >= 2 && j <= 4)) {
          matrix[startRow + i][startCol + j] = true
        }
      }
    }
  }

  _isInFinderPattern(row, col, moduleCount) {
    return (row < 8 && col < 8) ||
           (row < 8 && col >= moduleCount - 8) ||
           (row >= moduleCount - 8 && col < 8)
  }

  _hashCode(str) {
    let hash = 0
    for (let i = 0; i < str.length; i++) {
      hash = ((hash << 5) - hash) + str.charCodeAt(i)
      hash |= 0
    }
    return Math.abs(hash)
  }

  _pseudoRandom(seed, index) {
    const x = Math.sin(seed + index) * 10000
    return x - Math.floor(x)
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
