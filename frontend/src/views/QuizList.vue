<template>
  <v-container class="py-8">
    <h1 class="text-h4 font-weight-bold mb-6">Choose a Quiz</h1>

    <v-row>
      <v-col cols="12" sm="6" md="4" v-for="quiz in quizStore.quizzes" :key="quiz.id">
        <v-card rounded="xl" elevation="2" class="card-hover h-100 d-flex flex-column">
          <v-card-item>
            <v-chip :color="difficultyColor(quiz.difficulty)" size="small" variant="flat" class="mb-2">
              {{ difficultyLabel(quiz.difficulty) }}
            </v-chip>
            <v-card-title class="text-h6 font-weight-bold">{{ quiz.title }}</v-card-title>
            <v-card-subtitle>{{ quiz.description }}</v-card-subtitle>
          </v-card-item>

          <v-card-text class="flex-grow-1">
            <div class="d-flex align-center mb-2">
              <v-icon size="18" class="mr-2">mdi-help-circle-outline</v-icon>
              <span>{{ quiz.questionCount }} questions</span>
            </div>
            <div class="d-flex align-center">
              <v-icon size="18" class="mr-2">mdi-timer-outline</v-icon>
              <span>{{ quiz.timeLimitPerQuestion }}s per question</span>
            </div>
          </v-card-text>

          <v-card-actions class="pa-4 pt-0">
            <v-btn color="primary" variant="flat" rounded="lg" block @click="startQuiz(quiz.id)">
              <v-icon start>mdi-play</v-icon>
              Play Now
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    <v-overlay :model-value="quizStore.loading" class="align-center justify-center">
      <v-progress-circular indeterminate size="64" color="primary" />
    </v-overlay>
  </v-container>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuizStore } from '../stores/quiz'

const router = useRouter()
const quizStore = useQuizStore()

const difficultyLabel = (d) => ['Easy', 'Medium', 'Hard'][d]
const difficultyColor = (d) => ['success', 'warning', 'error'][d]

onMounted(() => {
  quizStore.fetchQuizzes()
})

function startQuiz(id) {
  router.push(`/quiz/${id}/play`)
}
</script>

<style scoped>
.card-hover {
  transition: transform 0.2s;
}
.card-hover:hover {
  transform: translateY(-4px);
}
</style>
