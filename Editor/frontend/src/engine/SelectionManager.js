export class SelectionManager {
  constructor(canvas, callbacks = {}) {
    this.canvas = canvas
    this.callbacks = callbacks
    this.selectedId = null

    this.canvas.on('selection:created', (e) => this._onSelectionCreated(e))
    this.canvas.on('selection:updated', (e) => this._onSelectionCreated(e))
    this.canvas.on('selection:cleared', () => this._onSelectionCleared())
  }

  _onSelectionCreated(e) {
    const obj = e.selected?.[0]
    if (obj && obj._elementId) {
      this.selectedId = obj._elementId
      this.callbacks.onSelected?.(this.selectedId)
    }
  }

  _onSelectionCleared() {
    this.selectedId = null
    this.callbacks.onSelectionCleared?.()
  }

  selectById(id) {
    const objects = this.canvas.getObjects()
    const obj = objects.find(o => o._elementId === id)
    if (obj) {
      this.canvas.setActiveObject(obj)
      this.canvas.renderAll()
    }
  }

  clearSelection() {
    this.canvas.discardActiveObject()
    this.canvas.renderAll()
  }

  destroy() {
    this.canvas.off('selection:created')
    this.canvas.off('selection:updated')
    this.canvas.off('selection:cleared')
  }
}
