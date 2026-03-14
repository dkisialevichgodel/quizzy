<template>
  <v-container class="py-8">
    <h1 class="text-h4 font-weight-bold mb-6">
      <v-icon class="mr-2">mdi-history</v-icon>
      My Quiz History
    </h1>

    <v-card rounded="xl" elevation="1">
      <v-table v-if="history.length" density="comfortable">
        <thead>
          <tr>
            <th>Quiz</th>
            <th>Difficulty</th>
            <th>Score</th>
            <th>Status</th>
            <th>Date</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="attempt in history" :key="attempt.id">
            <td class="font-weight-medium">{{ attempt.quizTitle }}</td>
            <td>
              <v-chip :color="difficultyColor(attempt.difficulty)" size="small" variant="flat">
                {{ difficultyLabel(attempt.difficulty) }}
              </v-chip>
            </td>
            <td>
              <span class="font-weight-bold text-primary">{{ attempt.totalScore }}</span>
              <span class="text-medium-emphasis"> / {{ attempt.maxPossibleScore }}</span>
            </td>
            <td>
              <v-chip :color="attempt.isCompleted ? 'success' : 'warning'" size="small" variant="flat">
                {{ attempt.isCompleted ? 'Completed' : 'In Progress' }}
              </v-chip>
            </td>
            <td class="text-medium-emphasis">{{ formatDate(attempt.startedAt) }}</td>
            <td>
              <v-btn v-if="attempt.isCompleted" icon size="small" variant="text"
                @click="router.push(`/quiz/${attempt.quizId}/result/${attempt.id}`)">
                <v-icon>mdi-eye</v-icon>
              </v-btn>
            </td>
          </tr>
        </tbody>
      </v-table>

      <v-card-text v-else class="text-center py-16">
        <v-icon size="64" color="grey" class="mb-4">mdi-clipboard-text-off-outline</v-icon>
        <p class="text-h6 text-medium-emphasis">No quiz attempts yet</p>
        <v-btn color="primary" variant="flat" class="mt-4" rounded="lg" @click="router.push('/quizzes')">
          Start Your First Quiz
        </v-btn>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '../api'

const router = useRouter()
const history = ref([])

const difficultyLabel = (d) => ['Easy', 'Medium', 'Hard'][d]
const difficultyColor = (d) => ['success', 'warning', 'error'][d]

function formatDate(dateStr) {
  return new Date(dateStr).toLocaleDateString('en-US', {
    year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit',
  })
}

onMounted(async () => {
  const { data } = await api.get('/attempts/history')
  history.value = data
})
</script>
