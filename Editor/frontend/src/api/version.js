import apiClient from './index'

export async function getVersions(templateId) {
  const result = await apiClient.get(`/templates/${templateId}/versions`)
  return result.data
}

export async function getVersion(templateId, versionId) {
  const result = await apiClient.get(`/templates/${templateId}/versions/${versionId}`)
  return result.data
}

export async function rollbackVersion(templateId, versionId) {
  const result = await apiClient.post(`/templates/${templateId}/versions/${versionId}/rollback`, {})
  return result.data
}

export async function getDiff(templateId, versionIdA, versionIdB) {
  const result = await apiClient.get(`/templates/${templateId}/versions/diff`, {
    params: { vidA: versionIdA, vidB: versionIdB }
  })
  return result.data
}
