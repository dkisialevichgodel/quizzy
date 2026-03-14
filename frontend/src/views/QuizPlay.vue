<template>
  <v-container class="py-8" max-width="800">
    <!-- Loading -->
    <div v-if="loading" class="text-center py-16">
      <v-progress-circular indeterminate size="64" color="primary" />
      <p class="mt-4 text-medium-emphasis">Loading quiz...</p>
    </div>

    <!-- Quiz Play -->
    <template v-else-if="started && currentQuestion">
      <!-- Progress -->
      <v-progress-linear :model-value="((currentIndex + 1) / questions.length) * 100" color="primary"
        rounded height="8" class="mb-4" />

      <div class="d-flex justify-space-between align-center mb-4">
        <v-chip variant="tonal" color="primary">
          Question {{ currentIndex + 1 }} / {{ questions.length }}
        </v-chip>
        <v-chip :color="timerColor" variant="flat">
          <v-icon start size="18">mdi-timer</v-icon>
          {{ timeLeft }}s
        </v-chip>
      </div>

      <!-- Question Card -->
      <v-card rounded="xl" elevation="3" class="mb-6">
        <v-card-text class="pa-6">
          <div class="d-flex align-center mb-1">
            <v-chip size="x-small" :color="currentQuestion.type === 0 ? 'secondary' : 'accent'" variant="flat" class="mr-2">
              {{ currentQuestion.type === 0 ? 'Multiple Choice' : 'Open Text' }}
            </v-chip>
            <v-chip size="x-small" variant="outlined">{{ currentQuestion.points }} pts</v-chip>
          </div>

          <h2 class="text-h5 font-weight-bold mt-3 mb-4">{{ currentQuestion.text }}</h2>

          <!-- Media -->
          <div v-if="currentQuestion.mediaId" class="mb-4">
            <img v-if="isImage(currentQuestion.mediaId)" :src="`/api/media/${currentQuestion.mediaId}`"
              class="rounded-lg" style="max-width: 100%; max-height: 300px;" />
            <audio v-else :src="`/api/media/${currentQuestion.mediaId}`" controls class="w-100" />
          </div>

          <!-- Multiple Choice -->
          <div v-if="currentQuestion.type === 0" class="mt-4">
            <v-btn v-for="option in currentQuestion.answerOptions" :key="option.id"
              :variant="selectedOptionId === option.id ? 'flat' : 'outlined'"
              :color="selectedOptionId === option.id ? 'primary' : ''"
              block class="mb-3 text-left justify-start" size="large" rounded="lg"
              @click="selectedOptionId = option.id">
              <div class="d-flex align-center w-100">
                <v-icon v-if="option.mediaId" class="mr-2">mdi-image</v-icon>
                <span>{{ option.text }}</span>
              </div>
            </v-btn>
          </div>

          <!-- Open Text -->
          <div v-else class="mt-4">
            <v-textarea v-model="textAnswer" label="Type your answer..."
              variant="outlined" rows="3" auto-grow />
          </div>
        </v-card-text>

        <v-card-actions class="pa-6 pt-0">
          <v-spacer />
          <v-btn color="primary" variant="flat" size="large" rounded="lg"
            :disabled="!canSubmit" @click="submitAnswer" :loading="submitting">
            {{ isLastQuestion ? 'Finish Quiz' : 'Next Question' }}
            <v-icon end>{{ isLastQuestion ? 'mdi-check-circle' : 'mdi-arrow-right' }}</v-icon>
          </v-btn>
        </v-card-actions>
      </v-card>
    </template>

    <!-- Start Screen -->
    <template v-else-if="!started">
      <v-card rounded="xl" elevation="3" class="text-center pa-8">
        <v-icon size="80" color="primary" class="mb-4">mdi-brain</v-icon>
        <h1 class="text-h4 font-weight-bold mb-2">Ready?</h1>
        <p class="text-body-1 text-medium-emphasis mb-6">
          This quiz has {{ questions.length }} questions. Answer as fast and accurately as you can!
        </p>
        <v-btn color="primary" size="x-large" rounded="pill" @click="startAttempt" :loading="submitting">
          <v-icon start>mdi-play</v-icon>
          Begin Quiz
        </v-btn>
      </v-card>
    </template>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, inject } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuizStore } from '../stores/quiz'
import api from '../api'

const route = useRoute()
const router = useRouter()
const quizStore = useQuizStore()
const notify = inject('snackbar')

const quizId = parseInt(route.params.id)
const loading = ref(true)
const started = ref(false)
const submitting = ref(false)
const questions = ref([])
const currentIndex = ref(0)
const attemptId = ref(null)
const selectedOptionId = ref(null)
const textAnswer = ref('')
const timeLeft = ref(30)
const questionStartTime = ref(0)
let timerInterval = null

const currentQuestion = computed(() => questions.value[currentIndex.value])
const isLastQuestion = computed(() => currentIndex.value === questions.value.length - 1)
const canSubmit = computed(() => {
  if (!currentQuestion.value) return false
  return currentQuestion.value.type === 0 ? selectedOptionId.value !== null : textAnswer.value.trim().length > 0
})
const timerColor = computed(() => {
  if (timeLeft.value > 15) return 'success'
  if (timeLeft.value > 5) return 'warning'
  return 'error'
})

onMounted(async () => {
  try {
    questions.value = await quizStore.fetchPlayQuestions(quizId)
  } catch {
    notify('Failed to load quiz', 'error')
    router.push('/quizzes')
  } finally {
    loading.value = false
  }
})

onUnmounted(() => {
  clearInterval(timerInterval)
})

function isImage() {
  return true // simplified; the backend serves the correct content type
}

async function startAttempt() {
  submitting.value = true
  try {
    const { data } = await api.post('/attempts/start', { quizId })
    attemptId.value = data.id
    started.value = true
    startTimer()
  } catch (e) {
    notify(e.response?.data?.error || 'Failed to start quiz', 'error')
  } finally {
    submitting.value = false
  }
}

function startTimer() {
  const q = currentQuestion.value
  timeLeft.value = q?.timeLimit || 30
  questionStartTime.value = Date.now()
  clearInterval(timerInterval)
  timerInterval = setInterval(() => {
    const elapsed = Math.floor((Date.now() - questionStartTime.value) / 1000)
    timeLeft.value = Math.max(0, (q?.timeLimit || 30) - elapsed)
    if (timeLeft.value === 0) {
      submitAnswer()
    }
  }, 200)
}

async function submitAnswer() {
  if (submitting.value) return
  submitting.value = true
  clearInterval(timerInterval)

  const timeSpent = Math.floor((Date.now() - questionStartTime.value) / 1000)

  try {
    await api.post(`/attempts/${attemptId.value}/answer`, {
      questionId: currentQuestion.value.id,
      selectedOptionId: selectedOptionId.value,
      textAnswer: textAnswer.value || null,
      timeSpent,
    })

    if (isLastQuestion.value) {
      await api.post(`/attempts/${attemptId.value}/complete`)
      router.push(`/quiz/${quizId}/result/${attemptId.value}`)
    } else {
      currentIndex.value++
      selectedOptionId.value = null
      textAnswer.value = ''
      startTimer()
    }
  } catch (e) {
    notify(e.response?.data?.error || 'Failed to submit answer', 'error')
  } finally {
    submitting.value = false
  }
}
</script>
