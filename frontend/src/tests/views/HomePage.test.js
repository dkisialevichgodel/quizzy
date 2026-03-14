import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { shallowMount, flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import HomePage from '../../views/HomePage.vue'
import { useLeaderboardStore } from '../../stores/leaderboard'
import { useAuthStore } from '../../stores/auth'

// Mock router
const mockRouterPush = vi.fn()
vi.mock('vue-router', async () => {
  const actual = await vi.importActual('vue-router')
  return {
    ...actual,
    useRouter: () => ({
      push: mockRouterPush
    })
  }
})

// Mock API
vi.mock('../../api')

describe('HomePage Component', () => {
  let wrapper
  let pinia
  let leaderboardStore
  let authStore

  const mountComponent = () => {
    const snackbarMock = vi.fn()
    
    return shallowMount(HomePage, {
      global: {
        plugins: [pinia],
        provide: {
          snackbar: snackbarMock
        },
        stubs: ['router-link']
      }
    })
  }

  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)
    leaderboardStore = useLeaderboardStore()
    authStore = useAuthStore()
    
    // Mock the store methods before tests run
    vi.spyOn(leaderboardStore, 'fetchAll').mockResolvedValue()
    
    vi.clearAllMocks()
    mockRouterPush.mockClear()
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('Component Data', () => {
    it('should have correct stats data', () => {
      wrapper = mountComponent()
      const stats = wrapper.vm.stats
      expect(stats).toHaveLength(3)
      expect(stats[0].label).toBe('Difficulty Levels')
    })

    it('should have correct difficulty level configuration', () => {
      wrapper = mountComponent()
      const levels = wrapper.vm.difficultyLevels

      expect(levels).toHaveLength(3)
      expect(levels[0]).toMatchObject({
        value: 0,
        label: 'Easy',
        color: 'success',
        time: 30
      })
      expect(levels[1]).toMatchObject({
        value: 1,
        label: 'Medium',
        color: 'warning',
        time: 25
      })
      expect(levels[2]).toMatchObject({
        value: 2,
        label: 'Hard',
        color: 'error',
        time: 20
      })
    })

    it('should have correct features data', () => {
      wrapper = mountComponent()
      const features = wrapper.vm.features
      expect(features).toHaveLength(4)
      expect(features[0].title).toBe('Smart Scoring')
    })
  })

  describe('Start Playing Action', () => {
    it('should redirect to /quizzes when authenticated', async () => {
      authStore.token = 'test-token'
      authStore.user = { id: 1, username: 'test' }
      
      wrapper = mountComponent()
      await wrapper.vm.handleStartGame()

      expect(mockRouterPush).toHaveBeenCalledWith('/quizzes')
    })

    it('should redirect to home with login query when not authenticated', async () => {
      authStore.token = null
      authStore.user = null
      
      wrapper = mountComponent()
      await wrapper.vm.handleStartGame()

      expect(mockRouterPush).toHaveBeenCalledWith({ query: { login: '1' } })
    })
  })

  describe('Lifecycle', () => {
    it('should fetch all leaderboards on mount', async () => {
      wrapper = mountComponent()
      await flushPromises()

      expect(leaderboardStore.fetchAll).toHaveBeenCalled()
    })
  })
})
