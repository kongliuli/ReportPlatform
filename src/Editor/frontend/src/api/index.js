import axios from 'axios'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

class ApiError extends Error {
  constructor(code, message) {
    super(message)
    this.code = code
    this.name = 'ApiError'
  }
}

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

let isRefreshing = false
let refreshSubscribers = []

function subscribeTokenRefresh(cb) {
  refreshSubscribers.push(cb)
}

function onTokenRefreshed(newToken) {
  refreshSubscribers.forEach(cb => cb(newToken))
  refreshSubscribers = []
}

apiClient.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

apiClient.interceptors.response.use(
  (response) => {
    const data = response.data
    if (data && data.code !== undefined && data.code !== 200) {
      return Promise.reject(new ApiError(data.code, data.message))
    }
    return data
  },
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      const authStore = useAuthStore()

      if (isRefreshing) {
        return new Promise((resolve) => {
          subscribeTokenRefresh((newToken) => {
            originalRequest.headers.Authorization = `Bearer ${newToken}`
            resolve(apiClient(originalRequest))
          })
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const result = await authStore.refreshAccessToken()
        onTokenRefreshed(result.accessToken)
        originalRequest.headers.Authorization = `Bearer ${result.accessToken}`
        return apiClient(originalRequest)
      } catch (refreshError) {
        authStore.logout()
        router.push({ path: '/login', query: { redirect: router.currentRoute.value.fullPath } })
        return Promise.reject(new ApiError(401, '登录已过期，请重新登录'))
      } finally {
        isRefreshing = false
      }
    }

    const code = error.response?.status || 500
    const message = error.response?.data?.message || error.message || '请求失败'
    return Promise.reject(new ApiError(code, message))
  }
)

export { ApiError }
export default apiClient
