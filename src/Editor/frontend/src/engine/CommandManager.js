export class CommandManager {
  constructor(limit = 50) {
    this.undoStack = []
    this.redoStack = []
    this.limit = limit
  }

  execute(command) {
    command.execute()
    this.undoStack.push(command)
    if (this.undoStack.length > this.limit) {
      this.undoStack.shift()
    }
    this.redoStack = []
  }

  undo() {
    if (this.undoStack.length === 0) return null
    const command = this.undoStack.pop()
    command.undo()
    this.redoStack.push(command)
    return command
  }

  redo() {
    if (this.redoStack.length === 0) return null
    const command = this.redoStack.pop()
    command.execute()
    this.undoStack.push(command)
    return command
  }

  get canUndo() {
    return this.undoStack.length > 0
  }

  get canRedo() {
    return this.redoStack.length > 0
  }

  get lastUndoDescription() {
    return this.undoStack[this.undoStack.length - 1]?.description || ''
  }

  clear() {
    this.undoStack = []
    this.redoStack = []
  }
}

export function createAddElementCommand(engine, element) {
  return {
    type: 'addElement',
    description: `添加${element.getElementType?.() || '元素'}`,
    execute: () => {
      engine.template.addElement(element)
      engine._addFabricObject(element)
      engine.canvas.renderAll()
    },
    undo: () => {
      engine.removeElement(element.id)
    }
  }
}

export function createRemoveElementCommand(engine, element) {
  return {
    type: 'removeElement',
    description: `删除${element.getElementType?.() || '元素'}`,
    execute: () => {
      engine.removeElement(element.id)
    },
    undo: () => {
      engine.template.addElement(element)
      engine._addFabricObject(element)
      engine.canvas.renderAll()
    }
  }
}

export function createUpdateElementCommand(engine, id, newProps, oldProps) {
  return {
    type: 'updateElement',
    description: '修改元素属性',
    execute: () => {
      engine.updateElement(id, newProps)
    },
    undo: () => {
      engine.updateElement(id, oldProps)
    }
  }
}

export function createMoveElementCommand(engine, id, newX, newY, oldX, oldY) {
  return {
    type: 'moveElement',
    description: '移动元素',
    execute: () => {
      engine.updateElement(id, { x: newX, y: newY })
    },
    undo: () => {
      engine.updateElement(id, { x: oldX, y: oldY })
    }
  }
}
