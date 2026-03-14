<template>
  <v-container class="py-8" max-width="800">
    <div v-if="loading" class="text-center py-16">
      <v-progress-circular indeterminate size="64" color="primary" />
    </div>

    <template v-else-if="result">
      <!-- Score Header -->
      <v-card rounded="xl" elevation="3" class="text-center pa-8 mb-6" :color="scoreColor" variant="tonal">
        <v-icon size="64" :color="scoreColor" class="mb-3">
          {{ scoreIcon }}
        </v-icon>
        <h1 class="text-h3 font-weight-black mb-2">
          {{ result.totalScore }} <span class="text-h5 text-medium-emphasis">/ {{ result.maxPossibleScore + maxTimeBonus }}</span>
        </h1>
        <p class="text-h6 text-medium-emphasis">Total Score</p>

        <v-row class="mt-4" justify="center">
          <v-col cols="4">
            <div class="text-h5 font-weight-bold text-primary">{{ result.baseScore }}</div>
            <div class="text-caption">Base Score</div>
          </v-col>
          <v-col cols="4">
            <div class="text-h5 font-weight-bold text-success">+{{ result.timeBonus }}</div>
            <div class="text-caption">Time Bonus</div>
          </v-col>
          <v-col cols="4">
            <div class="text-h5 font-weight-bold">{{ correctCount }}/{{ result.answers.length }}</div>
            <div class="text-caption">Correct</div>
          </v-col>
        </v-row>
      </v-card>

      <!-- Answers Breakdown -->
      <h2 class="text-h5 font-weight-bold mb-4">Answer Breakdown</h2>

      <v-card v-for="(answer, i) in result.answers" :key="answer.id" rounded="xl" class="mb-3" elevation="1"
        :color="answer.isCorrect ? 'success' : answer.pointsEarned > 0 ? 'warning' : 'error'" variant="tonal">
        <v-card-text class="pa-4">
          <div class="d-flex justify-space-between align-center mb-2">
            <v-chip size="small" :color="answer.isCorrect ? 'success' : answer.pointsEarned > 0 ? 'warning' : 'error'" variant="flat">
              Q{{ i + 1 }} · {{ answer.pointsEarned }} pts
            </v-chip>
            <v-chip size="x-small" variant="outlined">{{ answer.timeSpent }}s</v-chip>
          </div>

          <p class="font-weight-medium mb-2">{{ answer.questionText }}</p>

          <div v-if="answer.questionType === 0" class="text-body-2">
            <div>Your answer: <strong :class="answer.isCorrect ? 'text-success' : 'text-error'">{{ answer.selectedOptionText || 'No answer' }}</strong></div>
            <div v-if="!answer.isCorrect">Correct: <strong class="text-success">{{ answer.correctAnswer }}</strong></div>
          </div>
          <div v-else class="text-body-2">
            <div>Your answer: <strong>{{ answer.textAnswer || 'No answer' }}</strong></div>
            <div v-if="answer.similarityScore != null">
              Similarity: <v-chip size="x-small" :color="answer.similarityScore >= 0.7 ? 'success' : 'warning'" variant="flat">
                {{ (answer.similarityScore * 100).toFixed(0) }}%
              </v-chip>
            </div>
            <div>Expected: <strong class="text-success">{{ answer.correctAnswer }}</strong></div>
          </div>
        </v-card-text>
      </v-card>

      <!-- Actions -->
      <div class="d-flex gap-3 mt-6">
        <v-btn color="primary" variant="flat" rounded="lg" @click="router.push('/quizzes')">
          <v-icon start>mdi-replay</v-icon>
          Play Again
        </v-btn>
        <v-btn variant="outlined" rounded="lg" @click="router.push('/history')">
          <v-icon start>mdi-history</v-icon>
          History
        </v-btn>
        <v-btn variant="outlined" rounded="lg" @click="router.push('/')">
          <v-icon start>mdi-home</v-icon>
          Home
        </v-btn>
      </div>
    </template>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted, inject } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../api'

const route = useRoute()
const router = useRouter()
const notify = inject('snackbar')

const loading = ref(true)
const result = ref(null)

const correctCount = computed(() => result.value?.answers.filter((a) => a.isCorrect).length || 0)
const maxTimeBonus = computed(() => result.value ? result.value.answers.length * 5 : 0)
const scorePercent = computed(() => result.value ? result.value.totalScore / (result.value.maxPossibleScore + maxTimeBonus.value) : 0)
const scoreColor = computed(() => {
  if (scorePercent.value >= 0.8) return 'success'
  if (scorePercent.value >= 0.5) return 'warning'
  return 'error'
})
const scoreIcon = computed(() => {
  if (scorePercent.value >= 0.8) return 'mdi-trophy'
  if (scorePercent.value >= 0.5) return 'mdi-star-half-full'
  return 'mdi-emoticon-sad-outline'
})

onMounted(async () => {
  try {
    const { data } = await api.get(`/attempts/${route.params.attemptId}`)
    result.value = data
  } catch {
    notify('Failed to load results', 'error')
    router.push('/quizzes')
  } finally {
    loading.value = false
  }
})
</script>
