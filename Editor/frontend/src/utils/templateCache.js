const STORAGE_KEY_TEMPLATES = 'xinglin_templates'
const STORAGE_KEY_DRAFT_PREFIX = 'xinglin_draft_'
const CACHE_SIZE = 10

function trimCache(cache) {
  while (cache.length > CACHE_SIZE) cache.shift()
}

export function getCachedTemplates() {
  try {
    const data = localStorage.getItem(STORAGE_KEY_TEMPLATES)
    return data ? JSON.parse(data) : []
  } catch {
    return []
  }
}

export function setCachedTemplates(templates) {
  try {
    localStorage.setItem(STORAGE_KEY_TEMPLATES, JSON.stringify(templates))
  } catch (e) {
    console.warn('Failed to cache templates', e)
  }
}

export function getDraft(templateId) {
  try {
    const data = localStorage.getItem(STORAGE_KEY_DRAFT_PREFIX + templateId)
    return data ? JSON.parse(data) : null
  } catch {
    return null
  }
}

export function setDraft(templateId, content) {
  try {
    localStorage.setItem(STORAGE_KEY_DRAFT_PREFIX + templateId, JSON.stringify(content))
  } catch (e) {
    console.warn('Failed to save draft', e)
  }
}

export function clearDraft(templateId) {
  try {
    localStorage.removeItem(STORAGE_KEY_DRAFT_PREFIX + templateId)
  } catch (e) {
    console.warn('Failed to clear draft', e)
  }
}
