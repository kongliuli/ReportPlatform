import { Group, Rect, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

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

export class IconElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const iconSize = element.iconSize || 24

    const iconChar = ICON_PLACEHOLDERS[element.iconName] || element.iconName || '★'

    const fabricObj = new IText(iconChar, {
      left,
      top,
      fontSize: iconSize,
      fontFamily: 'Arial, sans-serif',
      fill: element.iconColor || '#000000',
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
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
    if (props.opacity !== undefined) fabricObj.set('opacity', props.opacity)
    if (props.rotation !== undefined) fabricObj.set('angle', props.rotation)
    if (props.x !== undefined || props.y !== undefined) {
      fabricObj.set({
        left: (props.x !== undefined ? props.x * MM_TO_PX : fabricObj.left - CANVAS_PADDING) + CANVAS_PADDING,
        top: (props.y !== undefined ? props.y * MM_TO_PX : fabricObj.top - CANVAS_PADDING) + CANVAS_PADDING
      })
    }
  }

  toModel(fabricObj) {
    return {
      x: round2((fabricObj.left - CANVAS_PADDING) / MM_TO_PX),
      y: round2((fabricObj.top - CANVAS_PADDING) / MM_TO_PX),
      iconSize: fabricObj.fontSize,
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
