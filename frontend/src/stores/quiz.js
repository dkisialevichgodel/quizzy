import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../api'

export const useQuizStore = defineStore('quiz', () => {
  const quizzes = ref([])
  const currentQuiz = ref(null)
  const loading = ref(false)

  async function fetchQuizzes() {
    loading.value = true
    try {
      const { data } = await api.get('/quizzes')
      quizzes.value = data
    } finally {
      loading.value = false
    }
  }

  async function fetchQuiz(id) {
    loading.value = true
    try {
      const { data } = await api.get(`/quizzes/${id}`)
      currentQuiz.value = data
      return data
    } finally {
      loading.value = false
    }
  }

  async function fetchPlayQuestions(id) {
    const { data } = await api.get(`/quizzes/${id}/play`)
    return data
  }

  async function createQuiz(quiz) {
    const { data } = await api.post('/quizzes', quiz)
    quizzes.value.push(data)
    return data
  }

  async function updateQuiz(id, quiz) {
    const { data } = await api.put(`/quizzes/${id}`, quiz)
    const index = quizzes.value.findIndex((q) => q.id === id)
    if (index !== -1) quizzes.value[index] = data
    return data
  }

  async function deleteQuiz(id) {
    await api.delete(`/quizzes/${id}`)
    quizzes.value = quizzes.value.filter((q) => q.id !== id)
  }

  return { quizzes, currentQuiz, loading, fetchQuizzes, fetchQuiz, fetchPlayQuestions, createQuiz, updateQuiz, deleteQuiz }
})
