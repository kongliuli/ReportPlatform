import apiClient from './index'

export async function loginApi(credentials) {
  const result = await apiClient.post('/auth/login', credentials)
  return result.data
}

export async function refreshTokenApi(refreshToken) {
  const result = await apiClient.post('/auth/refresh', { refreshToken })
  return result.data
}

export async function logoutApi() {
  await apiClient.post('/auth/logout')
}
