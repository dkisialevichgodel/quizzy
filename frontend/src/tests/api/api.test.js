import { describe, it, expect, beforeEach, vi } from 'vitest'
import axios from 'axios'

// Mock axios
vi.mock('axios')

// Mock router
const mockRouterPush = vi.fn()
vi.mock('../../router', () => ({
  default: {
    push: mockRouterPush
  }
}))

describe('API Module', () => {
  let api

  beforeEach(async () => {
    vi.clearAllMocks()
    localStorage.clear()
    
    // Reset modules to get fresh instance
    vi.resetModules()
    
    // Mock axios.create to return a mock instance
    const mockAxiosInstance = {
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() }
      },
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn()
    }
    
    axios.create.mockReturnValue(mockAxiosInstance)
    
    // Import api after mocking
    const apiModule = await import('../../api')
    api = apiModule.default
  })

  it('should create axios instance with correct config', () => {
    expect(axios.create).toHaveBeenCalledWith({
      baseURL: '/api',
      headers: { 'Content-Type': 'application/json' }
    })
  })

  it('should register request interceptor', () => {
    expect(api.interceptors.request.use).toHaveBeenCalled()
  })

  it('should register response interceptor', () => {
    expect(api.interceptors.response.use).toHaveBeenCalled()
  })

  describe('Request Interceptor', () => {
    it('should add Authorization header when token exists', async () => {
      const token = 'test-token-123'
      localStorage.getItem.mockReturnValue(token)

      // Get the request interceptor function
      const requestInterceptor = api.interceptors.request.use.mock.calls[0][0]
      
      const config = { headers: {} }
      const result = requestInterceptor(config)

      expect(result.headers.Authorization).toBe(`Bearer ${token}`)
    })

    it('should not add Authorization header when token does not exist', async () => {
      localStorage.getItem.mockReturnValue(null)

      const requestInterceptor = api.interceptors.request.use.mock.calls[0][0]
      
      const config = { headers: {} }
      const result = requestInterceptor(config)

      expect(result.headers.Authorization).toBeUndefined()
    })
  })

  describe('Response Interceptor', () => {
    it('should return response on success', async () => {
      const mockResponse = { data: { test: 'data' }, status: 200 }
      
      const successHandler = api.interceptors.response.use.mock.calls[0][0]
      const result = successHandler(mockResponse)

      expect(result).toBe(mockResponse)
    })

    it('should handle 401 errors by clearing storage and redirecting', async () => {
      const error = {
        response: {
          status: 401
        }
      }

      const errorHandler = api.interceptors.response.use.mock.calls[0][1]
      
      await expect(errorHandler(error)).rejects.toEqual(error)
      
      expect(localStorage.removeItem).toHaveBeenCalledWith('token')
      expect(localStorage.removeItem).toHaveBeenCalledWith('user')
      expect(mockRouterPush).toHaveBeenCalledWith('/')
    })

    it('should reject other errors without clearing storage', async () => {
      const error = {
        response: {
          status: 500
        }
      }

      const errorHandler = api.interceptors.response.use.mock.calls[0][1]
      
      await expect(errorHandler(error)).rejects.toEqual(error)
      
      expect(localStorage.removeItem).not.toHaveBeenCalled()
      expect(mockRouterPush).not.toHaveBeenCalled()
    })

    it('should handle errors without response object', async () => {
      const error = new Error('Network error')

      const errorHandler = api.interceptors.response.use.mock.calls[0][1]
      
      await expect(errorHandler(error)).rejects.toEqual(error)
      
      expect(localStorage.removeItem).not.toHaveBeenCalled()
      expect(mockRouterPush).not.toHaveBeenCalled()
    })
  })
})
