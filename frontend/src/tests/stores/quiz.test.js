import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useQuizStore } from '../../stores/quiz'
import api from '../../api'

vi.mock('../../api')

describe('Quiz Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('Initial State', () => {
    it('should have correct initial state', () => {
      const store = useQuizStore()
      
      expect(store.quizzes).toEqual([])
      expect(store.currentQuiz).toBeNull()
      expect(store.loading).toBe(false)
    })
  })

  describe('fetchQuizzes', () => {
    it('should fetch and set quizzes', async () => {
      const mockQuizzes = [
        { id: 1, title: 'Quiz 1', difficulty: 0 },
        { id: 2, title: 'Quiz 2', difficulty: 1 }
      ]
      api.get.mockResolvedValue({ data: mockQuizzes })

      const store = useQuizStore()
      await store.fetchQuizzes()

      expect(api.get).toHaveBeenCalledWith('/quizzes')
      expect(store.quizzes).toEqual(mockQuizzes)
      expect(store.loading).toBe(false)
    })

    it('should set loading state correctly', async () => {
      api.get.mockImplementation(() => new Promise(resolve => setTimeout(() => resolve({ data: [] }), 100)))

      const store = useQuizStore()
      const promise = store.fetchQuizzes()
      
      expect(store.loading).toBe(true)
      await promise
      expect(store.loading).toBe(false)
    })

    it('should handle errors and reset loading state', async () => {
      api.get.mockRejectedValue(new Error('Network error'))

      const store = useQuizStore()
      
      await expect(store.fetchQuizzes()).rejects.toThrow('Network error')
      expect(store.loading).toBe(false)
    })
  })

  describe('fetchQuiz', () => {
    it('should fetch and set current quiz', async () => {
      const mockQuiz = { id: 1, title: 'Test Quiz', difficulty: 0 }
      api.get.mockResolvedValue({ data: mockQuiz })

      const store = useQuizStore()
      const result = await store.fetchQuiz(1)

      expect(api.get).toHaveBeenCalledWith('/quizzes/1')
      expect(store.currentQuiz).toEqual(mockQuiz)
      expect(result).toEqual(mockQuiz)
      expect(store.loading).toBe(false)
    })

    it('should handle fetch errors', async () => {
      api.get.mockRejectedValue(new Error('Quiz not found'))

      const store = useQuizStore()
      
      await expect(store.fetchQuiz(999)).rejects.toThrow('Quiz not found')
      expect(store.loading).toBe(false)
    })
  })

  describe('fetchPlayQuestions', () => {
    it('should fetch play questions for a quiz', async () => {
      const mockQuestions = [
        { id: 1, text: 'Question 1' },
        { id: 2, text: 'Question 2' }
      ]
      api.get.mockResolvedValue({ data: mockQuestions })

      const store = useQuizStore()
      const result = await store.fetchPlayQuestions(1)

      expect(api.get).toHaveBeenCalledWith('/quizzes/1/play')
      expect(result).toEqual(mockQuestions)
    })
  })

  describe('createQuiz', () => {
    it('should create a quiz and add to list', async () => {
      const newQuiz = { title: 'New Quiz', difficulty: 1 }
      const createdQuiz = { id: 3, ...newQuiz }
      api.post.mockResolvedValue({ data: createdQuiz })

      const store = useQuizStore()
      const result = await store.createQuiz(newQuiz)

      expect(api.post).toHaveBeenCalledWith('/quizzes', newQuiz)
      expect(store.quizzes).toContainEqual(createdQuiz)
      expect(result).toEqual(createdQuiz)
    })
  })

  describe('updateQuiz', () => {
    it('should update quiz in the list', async () => {
      const existingQuiz = { id: 1, title: 'Old Title', difficulty: 0 }
      const updatedQuiz = { id: 1, title: 'New Title', difficulty: 1 }
      api.put.mockResolvedValue({ data: updatedQuiz })

      const store = useQuizStore()
      store.quizzes = [existingQuiz, { id: 2, title: 'Other Quiz' }]

      const result = await store.updateQuiz(1, updatedQuiz)

      expect(api.put).toHaveBeenCalledWith('/quizzes/1', updatedQuiz)
      expect(store.quizzes[0]).toEqual(updatedQuiz)
      expect(result).toEqual(updatedQuiz)
    })

    it('should handle updating non-existent quiz', async () => {
      const updatedQuiz = { id: 999, title: 'Updated' }
      api.put.mockResolvedValue({ data: updatedQuiz })

      const store = useQuizStore()
      store.quizzes = [{ id: 1, title: 'Quiz 1' }]

      await store.updateQuiz(999, updatedQuiz)

      expect(store.quizzes).toHaveLength(1)
      expect(store.quizzes[0].id).toBe(1)
    })
  })

  describe('deleteQuiz', () => {
    it('should delete quiz from the list', async () => {
      api.delete.mockResolvedValue({})

      const store = useQuizStore()
      store.quizzes = [
        { id: 1, title: 'Quiz 1' },
        { id: 2, title: 'Quiz 2' },
        { id: 3, title: 'Quiz 3' }
      ]

      await store.deleteQuiz(2)

      expect(api.delete).toHaveBeenCalledWith('/quizzes/2')
      expect(store.quizzes).toHaveLength(2)
      expect(store.quizzes.find(q => q.id === 2)).toBeUndefined()
    })

    it('should handle delete errors', async () => {
      api.delete.mockRejectedValue(new Error('Delete failed'))

      const store = useQuizStore()
      
      await expect(store.deleteQuiz(1)).rejects.toThrow('Delete failed')
    })
  })
})
