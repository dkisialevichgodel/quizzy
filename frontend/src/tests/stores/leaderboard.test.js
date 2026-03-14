import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useLeaderboardStore } from '../../stores/leaderboard'
import api from '../../api'

vi.mock('../../api')

describe('Leaderboard Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('Initial State', () => {
    it('should have correct initial state', () => {
      const store = useLeaderboardStore()
      
      expect(store.entries).toEqual({ 0: [], 1: [], 2: [] })
      expect(store.loading).toBe(false)
    })
  })

  describe('fetchLeaderboard', () => {
    it('should fetch leaderboard for specific difficulty', async () => {
      const mockEntries = [
        { userId: 1, username: 'player1', bestScore: 100, rank: 1 },
        { userId: 2, username: 'player2', bestScore: 90, rank: 2 }
      ]
      api.get.mockResolvedValue({ data: mockEntries })

      const store = useLeaderboardStore()
      await store.fetchLeaderboard(0)

      expect(api.get).toHaveBeenCalledWith('/leaderboard/0')
      expect(store.entries[0]).toEqual(mockEntries)
      expect(store.loading).toBe(false)
    })

    it('should handle different difficulty levels', async () => {
      const mockEntriesEasy = [{ userId: 1, username: 'p1', bestScore: 100, rank: 1 }]
      const mockEntriesMedium = [{ userId: 2, username: 'p2', bestScore: 80, rank: 1 }]
      const mockEntriesHard = [{ userId: 3, username: 'p3', bestScore: 60, rank: 1 }]

      const store = useLeaderboardStore()

      api.get.mockResolvedValue({ data: mockEntriesEasy })
      await store.fetchLeaderboard(0)
      expect(store.entries[0]).toEqual(mockEntriesEasy)

      api.get.mockResolvedValue({ data: mockEntriesMedium })
      await store.fetchLeaderboard(1)
      expect(store.entries[1]).toEqual(mockEntriesMedium)

      api.get.mockResolvedValue({ data: mockEntriesHard })
      await store.fetchLeaderboard(2)
      expect(store.entries[2]).toEqual(mockEntriesHard)
    })

    it('should set loading state correctly', async () => {
      api.get.mockImplementation(() => new Promise(resolve => setTimeout(() => resolve({ data: [] }), 100)))

      const store = useLeaderboardStore()
      const promise = store.fetchLeaderboard(0)
      
      expect(store.loading).toBe(true)
      await promise
      expect(store.loading).toBe(false)
    })

    it('should reset loading state on error', async () => {
      api.get.mockRejectedValue(new Error('Network error'))

      const store = useLeaderboardStore()
      
      await expect(store.fetchLeaderboard(0)).rejects.toThrow('Network error')
      expect(store.loading).toBe(false)
    })
  })

  describe('fetchAll', () => {
    it('should fetch all leaderboards concurrently', async () => {
      const mockEntriesEasy = [{ userId: 1, username: 'p1', bestScore: 100 }]
      const mockEntriesMedium = [{ userId: 2, username: 'p2', bestScore: 80 }]
      const mockEntriesHard = [{ userId: 3, username: 'p3', bestScore: 60 }]

      api.get.mockImplementation((url) => {
        if (url === '/leaderboard/0') return Promise.resolve({ data: mockEntriesEasy })
        if (url === '/leaderboard/1') return Promise.resolve({ data: mockEntriesMedium })
        if (url === '/leaderboard/2') return Promise.resolve({ data: mockEntriesHard })
      })

      const store = useLeaderboardStore()
      await store.fetchAll()

      expect(api.get).toHaveBeenCalledWith('/leaderboard/0')
      expect(api.get).toHaveBeenCalledWith('/leaderboard/1')
      expect(api.get).toHaveBeenCalledWith('/leaderboard/2')
      expect(api.get).toHaveBeenCalledTimes(3)

      expect(store.entries[0]).toEqual(mockEntriesEasy)
      expect(store.entries[1]).toEqual(mockEntriesMedium)
      expect(store.entries[2]).toEqual(mockEntriesHard)
      expect(store.loading).toBe(false)
    })

    it('should handle partial failures in fetchAll', async () => {
      api.get.mockImplementation((url) => {
        if (url === '/leaderboard/0') return Promise.resolve({ data: [{ userId: 1 }] })
        if (url === '/leaderboard/1') return Promise.reject(new Error('Failed'))
        if (url === '/leaderboard/2') return Promise.resolve({ data: [{ userId: 3 }] })
      })

      const store = useLeaderboardStore()
      
      await expect(store.fetchAll()).rejects.toThrow('Failed')
      expect(store.loading).toBe(false)
    })

    it('should set loading state correctly for fetchAll', async () => {
      api.get.mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve({ data: [] }), 50))
      )

      const store = useLeaderboardStore()
      const promise = store.fetchAll()
      
      expect(store.loading).toBe(true)
      await promise
      expect(store.loading).toBe(false)
    })
  })
})
