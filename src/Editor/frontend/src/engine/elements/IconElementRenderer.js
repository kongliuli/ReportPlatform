import { Group, Rect, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'
import { BaseElementRenderer } from './BaseElementRenderer'

const ICON_PLACEHOLDERS = {
  'star': '★',
  'heart': '♥',
  'check': '✓',
  'cross': '✕',
  'info': 'ℹ',
  'warning': '⚠',
  'error': '✖',
  'success': '✔',
  'user': '👤',
  'document': '📄',
  'folder': '📁',
  'settings': '⚙',
  'search': '🔍',
  'mail': '✉',
  'phone': '📞',
  'location': '📍',
  'calendar': '📅',
  'clock': '🕐',
  'lock': '🔒',
  'unlock': '🔓'
}

export class IconElementRenderer extends BaseElementRenderer {
  create(canvas, element, mmToPx) {
    const iconSize = element.iconSize || 24

    const iconChar = ICON_PLACEHOLDERS[element.iconName] || element.iconName || '★'

    const fabricObj = new IText(iconChar, {
      ...this._applyCommonOptions(element, mmToPx),
      fontSize: iconSize,
      fontFamily: 'Arial, sans-serif',
      fill: element.iconColor || '#000000',
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0
    })

    return fabricObj
  }

  update(fabricObj, props) {
    if (props.iconName !== undefined) {
      const iconChar = ICON_PLACEHOLDERS[props.iconName] || props.iconName || '★'
      fabricObj.set('text', iconChar)
    }
    if (props.iconSize !== undefined) fabricObj.set('fontSize', props.iconSize)
    if (props.iconColor !== undefined) fabricObj.set('fill', props.iconColor)
    this._applyCommonUpdate(fabricObj, props)
  }

  toModel(fabricObj) {
    return {
      ...this._applyCommonToModel(fabricObj),
      iconSize: fabricObj.fontSize,
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
