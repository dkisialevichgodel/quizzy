import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../../stores/auth'
import api from '../../api'

vi.mock('../../api')

describe('Auth Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    vi.clearAllMocks()
  })

  describe('Initial State', () => {
    it('should initialize with null token and user when localStorage is empty', () => {
      localStorage.getItem.mockReturnValue(null)
      const store = useAuthStore()
      
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(store.isAuthenticated).toBe(false)
      expect(store.isAdmin).toBe(false)
    })

    it('should initialize from localStorage when available', () => {
      const mockToken = 'test-token-123'
      const mockUser = { id: 1, username: 'testuser', email: 'test@test.com', role: 'User' }
      
      localStorage.getItem.mockImplementation((key) => {
        if (key === 'token') return mockToken
        if (key === 'user') return JSON.stringify(mockUser)
        return null
      })

      const store = useAuthStore()
      
      expect(store.token).toBe(mockToken)
      expect(store.user).toEqual(mockUser)
      expect(store.isAuthenticated).toBe(true)
      expect(store.isAdmin).toBe(false)
    })

    it('should identify admin users correctly', () => {
      const mockUser = { id: 1, username: 'admin', email: 'admin@test.com', role: 'Admin' }
      localStorage.getItem.mockImplementation((key) => {
        if (key === 'token') return 'token'
        if (key === 'user') return JSON.stringify(mockUser)
        return null
      })

      const store = useAuthStore()
      
      expect(store.isAdmin).toBe(true)
    })
  })

  describe('login', () => {
    it('should successfully login and store credentials', async () => {
      const mockResponse = {
        data: {
          token: 'new-token',
          user: { id: 1, username: 'testuser', email: 'test@test.com', role: 'User' }
        }
      }
      api.post.mockResolvedValue(mockResponse)

      const store = useAuthStore()
      const result = await store.login('test@test.com', 'password123')

      expect(api.post).toHaveBeenCalledWith('/auth/login', {
        email: 'test@test.com',
        password: 'password123'
      })
      expect(store.token).toBe('new-token')
      expect(store.user).toEqual(mockResponse.data.user)
      expect(localStorage.setItem).toHaveBeenCalledWith('token', 'new-token')
      expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify(mockResponse.data.user))
      expect(result).toEqual(mockResponse.data)
    })

    it('should throw error on login failure', async () => {
      api.post.mockRejectedValue(new Error('Invalid credentials'))

      const store = useAuthStore()
      // Ensure store starts with null state
      store.token = null
      store.user = null
      
      await expect(store.login('wrong@test.com', 'wrong')).rejects.toThrow('Invalid credentials')
      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
    })
  })

  describe('register', () => {
    it('should successfully register and store credentials', async () => {
      const mockResponse = {
        data: {
          token: 'new-token',
          user: { id: 2, username: 'newuser', email: 'new@test.com', role: 'User' }
        }
      }
      api.post.mockResolvedValue(mockResponse)

      const store = useAuthStore()
      const result = await store.register('newuser', 'new@test.com', 'password123')

      expect(api.post).toHaveBeenCalledWith('/auth/register', {
        username: 'newuser',
        email: 'new@test.com',
        password: 'password123'
      })
      expect(store.token).toBe('new-token')
      expect(store.user).toEqual(mockResponse.data.user)
      expect(result).toEqual(mockResponse.data)
    })

    it('should throw error on registration failure', async () => {
      api.post.mockRejectedValue(new Error('Email already exists'))

      const store = useAuthStore()
      
      await expect(store.register('user', 'exists@test.com', 'pass')).rejects.toThrow('Email already exists')
    })
  })

  describe('logout', () => {
    it('should clear all auth data', () => {
      const mockUser = { id: 1, username: 'test', role: 'User' }
      localStorage.getItem.mockImplementation((key) => {
        if (key === 'token') return 'token'
        if (key === 'user') return JSON.stringify(mockUser)
        return null
      })

      const store = useAuthStore()
      expect(store.isAuthenticated).toBe(true)

      store.logout()

      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
      expect(localStorage.removeItem).toHaveBeenCalledWith('token')
      expect(localStorage.removeItem).toHaveBeenCalledWith('user')
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('fetchMe', () => {
    it('should update user data on success', async () => {
      const mockUser = { id: 1, username: 'updated', email: 'test@test.com', role: 'Admin' }
      api.get.mockResolvedValue({ data: mockUser })

      const store = useAuthStore()
      await store.fetchMe()

      expect(api.get).toHaveBeenCalledWith('/auth/me')
      expect(store.user).toEqual(mockUser)
      expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify(mockUser))
    })

    it('should logout on fetch failure', async () => {
      api.get.mockRejectedValue(new Error('Unauthorized'))
      
      const store = useAuthStore()
      store.token = 'old-token'
      
      await store.fetchMe()

      expect(store.token).toBeNull()
      expect(store.user).toBeNull()
    })
  })
})
