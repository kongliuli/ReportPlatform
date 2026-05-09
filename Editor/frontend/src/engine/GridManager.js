import { Line } from 'fabric'
import { MM_TO_PX } from '@/utils/constants'

export class GridManager {
  constructor(canvas, pageWidthPx, pageHeightPx, padding = 20) {
    this.canvas = canvas
    this.pageWidthPx = pageWidthPx
    this.pageHeightPx = pageHeightPx
    this.padding = padding
    this.gridSizeMm = 5
    this.gridLines = []
    this.isVisible = false
  }

  show() {
    if (this.isVisible) return
    this.isVisible = true

    const gridSizePx = this.gridSizeMm * MM_TO_PX
    const offset = this.padding

    for (let x = 0; x <= this.pageWidthPx; x += gridSizePx) {
      const line = new Line([x + offset, offset, x + offset, this.pageHeightPx + offset], {
        stroke: '#c8c8c8',
        strokeWidth: 0.5,
        selectable: false,
        evented: false
      })
      this.canvas.add(line)
      this.canvas.moveTo(line, 1)
      this.gridLines.push(line)
    }

    for (let y = 0; y <= this.pageHeightPx; y += gridSizePx) {
      const line = new Line([offset, y + offset, this.pageWidthPx + offset, y + offset], {
        stroke: '#c8c8c8',
        strokeWidth: 0.5,
        selectable: false,
        evented: false
      })
      this.canvas.add(line)
      this.canvas.moveTo(line, 1)
      this.gridLines.push(line)
    }

    this.canvas.renderAll()
  }

  hide() {
    if (!this.isVisible) return
    this.isVisible = false

    this.gridLines.forEach(line => this.canvas.remove(line))
    this.gridLines = []
    this.canvas.renderAll()
  }

  toggle() {
    if (this.isVisible) {
      this.hide()
    } else {
      this.show()
    }
  }
}
