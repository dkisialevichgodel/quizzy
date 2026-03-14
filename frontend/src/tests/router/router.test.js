import { describe, it, expect, beforeEach, vi } from 'vitest'
import { createRouter, createWebHistory } from 'vue-router'

// We will test the router configuration directly
describe('Router', () => {
  let router

  beforeEach(async () => {
    vi.clearAllMocks()
    localStorage.clear()
    
    // Import router fresh for each test
    vi.resetModules()
    const routerModule = await import('../../router')
    router = routerModule.default
  })

  describe('Route Configuration', () => {
    it('should have correct number of routes', () => {
      expect(router.getRoutes()).toHaveLength(7)
    })

    it('should configure Home route correctly', () => {
      const route = router.getRoutes().find(r => r.name === 'Home')
      expect(route).toBeDefined()
      expect(route.path).toBe('/')
      expect(route.meta.requiresAuth).toBeUndefined()
    })

    it('should configure Quizzes route with auth requirement', () => {
      const route = router.getRoutes().find(r => r.name === 'Quizzes')
      expect(route).toBeDefined()
      expect(route.path).toBe('/quizzes')
      expect(route.meta.requiresAuth).toBe(true)
    })

    it('should configure QuizPlay route with auth requirement', () => {
      const route = router.getRoutes().find(r => r.name === 'QuizPlay')
      expect(route).toBeDefined()
      expect(route.path).toBe('/quiz/:id/play')
      expect(route.meta.requiresAuth).toBe(true)
    })

    it('should configure QuizResult route with auth requirement', () => {
      const route = router.getRoutes().find(r => r.name === 'QuizResult')
      expect(route).toBeDefined()
      expect(route.path).toBe('/quiz/:quizId/result/:attemptId')
      expect(route.meta.requiresAuth).toBe(true)
    })

    it('should configure History route with auth requirement', () => {
      const route = router.getRoutes().find(r => r.name === 'History')
      expect(route).toBeDefined()
      expect(route.path).toBe('/history')
      expect(route.meta.requiresAuth).toBe(true)
    })

    it('should configure Admin route with auth and admin requirements', () => {
      const route = router.getRoutes().find(r => r.name === 'Admin')
      expect(route).toBeDefined()
      expect(route.path).toBe('/admin')
      expect(route.meta.requiresAuth).toBe(true)
      expect(route.meta.requiresAdmin).toBe(true)
    })

    it('should configure AdminQuizEditor route with auth and admin requirements', () => {
      const route = router.getRoutes().find(r => r.name === 'AdminQuizEditor')
      expect(route).toBeDefined()
      expect(route.path).toBe('/admin/quiz/:id')
      expect(route.meta.requiresAuth).toBe(true)
      expect(route.meta.requiresAdmin).toBe(true)
    })
  })

  describe('Navigation Guards', () => {
    it('should redirect to Home with login query when accessing protected route without auth', async () => {
      localStorage.getItem.mockReturnValue(null)

      const result = await router.push('/quizzes')
      
      expect(result).toBeUndefined()
      expect(router.currentRoute.value.name).toBe('Home')
      expect(router.currentRoute.value.query.login).toBe('1')
    })

    it('should allow access to protected routes when authenticated', async () => {
      localStorage.getItem.mockImplementation((key) => {
        if (key === 'token') return 'valid-token'
        if (key === 'user') return JSON.stringify({ role: 'User' })
        return null
      })

      await router.push('/quizzes')
      
      expect(router.currentRoute.value.name).toBe('Quizzes')
    })

    it('should redirect non-admin users from admin routes', async () => {
      localStorage.getItem.mockImplementation((key) => {
        if (key === 'token') return 'valid-token'
        if (key === 'user') return JSON.stringify({ role: 'User' })
        return null
      })

      await router.push('/admin')
      
      expect(router.currentRoute.value.name).toBe('Home')
      expect(router.currentRoute.value.query.login).toBeUndefined()
    })

    it('should allow admin users to access admin routes', async () => {
      localStorage.getItem.mockImplementation((key) => {
        if (key === 'token') return 'admin-token'
        if (key === 'user') return JSON.stringify({ role: 'Admin' })
        return null
      })

      await router.push('/admin')
      
      expect(router.currentRoute.value.name).toBe('Admin')
    })

    it('should allow access to public routes without authentication', async () => {
      localStorage.getItem.mockReturnValue(null)

      await router.push('/')
      
      expect(router.currentRoute.value.name).toBe('Home')
    })

    it('should handle navigation to admin quiz editor for admins', async () => {
      localStorage.getItem.mockImplementation((key) => {
        if (key === 'token') return 'admin-token'
        if (key === 'user') return JSON.stringify({ role: 'Admin' })
        return null
      })

      await router.push('/admin/quiz/1')
      
      expect(router.currentRoute.value.name).toBe('AdminQuizEditor')
      expect(router.currentRoute.value.params.id).toBe('1')
    })

    it('should redirect regular users from admin quiz editor', async () => {
      localStorage.getItem.mockImplementation((key) => {
        if (key === 'token') return 'user-token'
        if (key === 'user') return JSON.stringify({ role: 'User' })
        return null
      })

      await router.push('/admin/quiz/1')
      
      expect(router.currentRoute.value.name).toBe('Home')
    })
  })
})
