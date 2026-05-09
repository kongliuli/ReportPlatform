import { Group, Rect, Line, IText } from 'fabric'
import { MM_TO_PX, CANVAS_PADDING, round2 } from '@/utils/constants'

const CHART_COLORS = ['#409EFF', '#67C23A', '#E6A23C', '#F56C6C', '#909399']

export class ChartElementRenderer {
  create(canvas, element, mmToPx) {
    const left = mmToPx(element.x) + CANVAS_PADDING
    const top = mmToPx(element.y) + CANVAS_PADDING
    const width = mmToPx(element.width)
    const height = mmToPx(element.height)
    const chartType = element.chartType || 'bar'

    const objects = []
    const padding = { top: 20, right: 20, bottom: 30, left: 40 }
    const chartWidth = width - padding.left - padding.right
    const chartHeight = height - padding.top - padding.bottom

    const bgRect = new Rect({
      left: 0,
      top: 0,
      width,
      height,
      fill: element.backgroundColor || 'transparent',
      stroke: element.borderColor || '#E4E7ED',
      strokeWidth: 1
    })
    objects.push(bgRect)

    const xAxisLine = new Line([padding.left, height - padding.bottom, width - padding.right, height - padding.bottom], {
      stroke: '#DCDFE6',
      strokeWidth: 1
    })
    objects.push(xAxisLine)

    const yAxisLine = new Line([padding.left, padding.top, padding.left, height - padding.bottom], {
      stroke: '#DCDFE6',
      strokeWidth: 1
    })
    objects.push(yAxisLine)

    const series = element.series || [{ data: [30, 50, 40, 60, 45] }]
    const maxData = Math.max(...series.flatMap(s => s.data || []))

    switch (chartType) {
      case 'bar':
        this._drawBarChart(objects, series, chartWidth, chartHeight, padding, maxData)
        break
      case 'line':
        this._drawLineChart(objects, series, chartWidth, chartHeight, padding, maxData)
        break
      case 'pie':
        this._drawPieChart(objects, series, width, height)
        break
    }

    const group = new Group(objects, {
      left,
      top,
      width,
      height,
      opacity: element.opacity ?? 1,
      angle: element.rotation || 0,
      borderColor: '#409eff',
      cornerColor: '#409eff',
      cornerSize: 8,
      transparentCorners: false
    })

    return group
  }

  _drawBarChart(objects, series, chartWidth, chartHeight, padding, maxData) {
    const data = series[0]?.data || [30, 50, 40, 60, 45]
    const barCount = data.length
    const barWidth = chartWidth / barCount * 0.6
    const gap = chartWidth / barCount * 0.4

    data.forEach((value, index) => {
      const barHeight = (value / maxData) * chartHeight
      const rect = new Rect({
        left: padding.left + index * (barWidth + gap) + gap / 2,
        top: padding.top + chartHeight - barHeight,
        width: barWidth,
        height: barHeight,
        fill: CHART_COLORS[index % CHART_COLORS.length]
      })
      objects.push(rect)
    })
  }

  _drawLineChart(objects, series, chartWidth, chartHeight, padding, maxData) {
    const data = series[0]?.data || [30, 50, 40, 60, 45]
    const pointGap = chartWidth / (data.length - 1)

    const points = data.map((value, index) => ({
      x: padding.left + index * pointGap,
      y: padding.top + chartHeight - (value / maxData) * chartHeight
    }))

    for (let i = 0; i < points.length - 1; i++) {
      const line = new Line([points[i].x, points[i].y, points[i + 1].x, points[i + 1].y], {
        stroke: CHART_COLORS[0],
        strokeWidth: 2
      })
      objects.push(line)
    }
  }

  _drawPieChart(objects, series, width, height) {
    const data = series[0]?.data || [30, 50, 40, 60, 45]
    const total = data.reduce((sum, v) => sum + v, 0)
    const centerX = width / 2
    const centerY = height / 2
    const radius = Math.min(width, height) / 3

    let startAngle = -Math.PI / 2

    data.forEach((value, index) => {
      const angle = (value / total) * Math.PI * 2
      const endAngle = startAngle + angle

      const x1 = centerX + radius * Math.cos(startAngle)
      const y1 = centerY + radius * Math.sin(startAngle)
      const x2 = centerX + radius * Math.cos(endAngle)
      const y2 = centerY + radius * Math.sin(endAngle)

      const largeArc = angle > Math.PI ? 1 : 0

      startAngle = endAngle
    })
  }

  update(fabricObj, props) {
    if (props.chartType !== undefined || props.series !== undefined) {
      fabricObj.dirty = true
    }
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
      width: round2(fabricObj.getScaledWidth() / MM_TO_PX),
      height: round2(fabricObj.getScaledHeight() / MM_TO_PX),
      rotation: round2(fabricObj.angle || 0),
      opacity: fabricObj.opacity
    }
  }
}
