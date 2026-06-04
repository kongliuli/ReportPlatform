import { Rect, Text, Group } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

export class TableElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const rows = element.rows || 3
    const cols = element.columns || 3
    const totalWidth = mmToPx(element.width)
    const totalHeight = mmToPx(element.height)
    const cellWidth = totalWidth / cols
    const cellHeight = totalHeight / rows
    const objects = []

    for (let r = 0; r < rows; r++) {
      for (let c = 0; c < cols; c++) {
        const rect = new Rect({
          left: c * cellWidth,
          top: r * cellHeight,
          width: cellWidth,
          height: cellHeight,
          fill: r === 0 && element.hasHeader ? '#f0f0f0' : 'transparent',
          stroke: '#000000',
          strokeWidth: element.tableBorder || 1
        })
        objects.push(rect)

        const cellText = element.cellData?.[r]?.[c] || ''
        if (cellText || (r === 0 && element.hasHeader)) {
          const text = new Text(cellText || '', {
            left: c * cellWidth + 4,
            top: r * cellHeight + 4,
            fontSize: element.fontSize || 10,
            fontFamily: element.fontFamily || 'SimSun',
            fill: element.foregroundColor || '#000000'
          })
          objects.push(text)
        }
      }
    }

    const group = new Group(objects, {
      ...this._applyCommonOptions(element, mmToPx),
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

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
