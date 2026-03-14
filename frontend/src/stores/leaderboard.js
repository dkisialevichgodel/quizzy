import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../api'

export const useLeaderboardStore = defineStore('leaderboard', () => {
  const entries = ref({ 0: [], 1: [], 2: [] })
  const loading = ref(false)

  async function fetchLeaderboard(difficulty) {
    loading.value = true
    try {
      const { data } = await api.get(`/leaderboard/${difficulty}`)
      entries.value[difficulty] = data
    } finally {
      loading.value = false
    }
  }

  async function fetchAll() {
    loading.value = true
    try {
      const [easy, medium, hard] = await Promise.all([
        api.get('/leaderboard/0'),
        api.get('/leaderboard/1'),
        api.get('/leaderboard/2'),
      ])
      entries.value[0] = easy.data
      entries.value[1] = medium.data
      entries.value[2] = hard.data
    } finally {
      loading.value = false
    }
  }

  return { entries, loading, fetchLeaderboard, fetchAll }
})
