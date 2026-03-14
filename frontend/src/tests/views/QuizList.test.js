import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { shallowMount, flushPromises } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import QuizList from '../../views/QuizList.vue'
import { useQuizStore } from '../../stores/quiz'

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

describe('QuizList Component', () => {
  let wrapper
  let pinia
  let quizStore

  const mountComponent = () => {
    return shallowMount(QuizList, {
      global: {
        plugins: [pinia]
      }
    })
  }

  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)
    quizStore = useQuizStore()
    
    // Mock the store methods before tests run
    vi.spyOn(quizStore, 'fetchQuizzes').mockResolvedValue()
    
    vi.clearAllMocks()
    mockRouterPush.mockClear()
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('Lifecycle', () => {
    it('should fetch quizzes on mount', async () => {
      wrapper = mountComponent()
      await flushPromises()

      expect(quizStore.fetchQuizzes).toHaveBeenCalled()
    })
  })

  describe('Quiz Navigation', () => {
    it('should navigate to quiz play page with correct ID', async () => {
      wrapper = mountComponent()
      
      await wrapper.vm.startQuiz(1)

      expect(mockRouterPush).toHaveBeenCalledWith('/quiz/1/play')
    })

    it('should navigate with different quiz IDs', async () => {
      wrapper = mountComponent()
      
      await wrapper.vm.startQuiz(5)

      expect(mockRouterPush).toHaveBeenCalledWith('/quiz/5/play')
    })
  })

  describe('Helper Functions', () => {
    it('should return correct difficulty labels', () => {
      wrapper = mountComponent()
      
      expect(wrapper.vm.difficultyLabel(0)).toBe('Easy')
      expect(wrapper.vm.difficultyLabel(1)).toBe('Medium')
      expect(wrapper.vm.difficultyLabel(2)).toBe('Hard')
    })

    it('should return correct difficulty colors', () => {
      wrapper = mountComponent()
      
      expect(wrapper.vm.difficultyColor(0)).toBe('success')
      expect(wrapper.vm.difficultyColor(1)).toBe('warning')
      expect(wrapper.vm.difficultyColor(2)).toBe('error')
    })
  })

  describe('Store Integration', () => {
    it('should use quiz store', () => {
      wrapper = mountComponent()
      
      expect(wrapper.vm.quizStore).toBeDefined()
      expect(wrapper.vm.quizStore.quizzes).toBeDefined()
      expect(wrapper.vm.quizStore.loading).toBeDefined()
    })
  })
})
