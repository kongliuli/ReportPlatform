import apiClient from './index'

export async function getTemplates(params) {
  const result = await apiClient.get('/templates', { params })
  return result.data
}

export async function getTemplate(id) {
  const result = await apiClient.get(`/templates/${id}`)
  return result.data
}

export async function createTemplateApi(data) {
  const result = await apiClient.post('/templates', data)
  return result.data
}

export async function updateTemplateApi(id, data) {
  const result = await apiClient.put(`/templates/${id}`, data)
  return result.data
}

export async function deleteTemplateApi(id) {
  const result = await apiClient.delete(`/templates/${id}`)
  return result.data
}
