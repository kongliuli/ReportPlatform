import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

export class BaseElementRenderer {
  /**
   * Returns common Fabric.js options for element positioning and selection controls.
   * @param {Object} element - The element model
   * @param {Function} mmToPx - Millimeter to pixel conversion function
   * @returns {Object} Common fabric options
   */
  _applyCommonOptions(element, mmToPx) {
    return {
      left: mmToPx(element.x) + CANVAS_PADDING,
      top: mmToPx(element.y) + CANVAS_PADDING,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
    }
  }

  /**
   * Applies common update properties (position, opacity, rotation) to a fabric object.
   * @param {Object} fabricObj - The fabric.js object
   * @param {Object} props - The properties to update
   */
  _applyCommonUpdate(fabricObj, props) {
    if (props.opacity !== undefined) {
      fabricObj.set('opacity', props.opacity)
    }
    if (props.rotation !== undefined) {
      fabricObj.set('angle', props.rotation)
    }
    if (props.x !== undefined || props.y !== undefined) {
      fabricObj.set({
        left: (props.x !== undefined ? props.x * MM_TO_PX : fabricObj.left - CANVAS_PADDING) + CANVAS_PADDING,
        top: (props.y !== undefined ? props.y * MM_TO_PX : fabricObj.top - CANVAS_PADDING) + CANVAS_PADDING
      })
    }
  }

  /**
   * Returns common model properties (x, y) extracted from a fabric object.
   * @param {Object} fabricObj - The fabric.js object
   * @returns {Object} Common model properties
   */
  _applyCommonToModel(fabricObj) {
    return {
      x: round2((fabricObj.left - CANVAS_PADDING) / MM_TO_PX),
      y: round2((fabricObj.top - CANVAS_PADDING) / MM_TO_PX)
    }
  }
}
