import { MM_TO_PX, CANVAS_PADDING } from '@/utils/constants'

export function captureCanvasAsImage(canvasEngine) {
  if (!canvasEngine?.canvas) return null

  const canvas = canvasEngine.canvas
  const template = canvasEngine.template

  const pageWidthPx = template.pageWidth * MM_TO_PX
  const pageHeightPx = template.pageHeight * MM_TO_PX

  const originalZoom = canvas.getZoom()
  canvas.setZoom(1)
  canvas.renderAll()

  const dataUrl = canvas.toDataURL({
    format: 'png',
    quality: 1,
    left: CANVAS_PADDING,
    top: CANVAS_PADDING,
    width: pageWidthPx,
    height: pageHeightPx
  })

  canvas.setZoom(originalZoom)
  canvas.renderAll()

  return dataUrl
}

export function downloadImage(dataUrl, filename) {
  const a = document.createElement('a')
  a.href = dataUrl
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
}

export function printCanvasAsPdf(canvasEngine, templateName) {
  const dataUrl = captureCanvasAsImage(canvasEngine)
  if (!dataUrl) return

  const template = canvasEngine.template
  const widthMm = template.pageWidth
  const heightMm = template.pageHeight

  const iframe = document.createElement('iframe')
  iframe.style.position = 'fixed'
  iframe.style.right = '0'
  iframe.style.bottom = '0'
  iframe.style.width = '0'
  iframe.style.height = '0'
  iframe.style.border = 'none'
  document.body.appendChild(iframe)

  const doc = iframe.contentDocument || iframe.contentWindow.document
  doc.write(`<!DOCTYPE html>
<html>
<head>
<title>${templateName || '模板预览'}</title>
<style>
  @page { size: ${widthMm}mm ${heightMm}mm; margin: 0; }
  body { margin: 0; padding: 0; display: flex; justify-content: center; }
  img { width: ${widthMm}mm; height: ${heightMm}mm; }
  @media print { body { -webkit-print-color-adjust: exact; print-color-adjust: exact; } }
</style>
</head>
<body>
<img src="${dataUrl}" onload="window.print();" />
</body>
</html>`)
  doc.close()

  setTimeout(() => document.body.removeChild(iframe), 60000)
}
