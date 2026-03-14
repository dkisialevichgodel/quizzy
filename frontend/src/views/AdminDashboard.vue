<template>
  <v-container class="py-8">
    <div class="d-flex justify-space-between align-center mb-6">
      <h1 class="text-h4 font-weight-bold">
        <v-icon class="mr-2" color="accent">mdi-shield-crown</v-icon>
        Admin Dashboard
      </h1>
      <v-btn color="primary" variant="flat" rounded="lg" @click="showCreateDialog = true">
        <v-icon start>mdi-plus</v-icon>
        New Quiz
      </v-btn>
    </div>

    <v-row>
      <v-col cols="12" sm="6" md="4" v-for="quiz in quizStore.quizzes" :key="quiz.id">
        <v-card rounded="xl" elevation="2" class="h-100 d-flex flex-column">
          <v-card-item>
            <div class="d-flex align-center gap-2 mb-1">
              <v-chip :color="difficultyColor(quiz.difficulty)" size="small" variant="flat">
                {{ difficultyLabel(quiz.difficulty) }}
              </v-chip>
              <v-chip :color="quiz.isActive ? 'success' : 'grey'" size="x-small" variant="flat">
                {{ quiz.isActive ? 'Active' : 'Inactive' }}
              </v-chip>
            </div>
            <v-card-title class="text-h6">{{ quiz.title }}</v-card-title>
            <v-card-subtitle>{{ quiz.questionCount }} questions · {{ quiz.timeLimitPerQuestion }}s limit</v-card-subtitle>
          </v-card-item>

          <v-card-actions class="pa-4 pt-0 mt-auto">
            <v-btn variant="flat" color="primary" rounded="lg" size="small" @click="router.push(`/admin/quiz/${quiz.id}`)">
              <v-icon start>mdi-pencil</v-icon> Edit
            </v-btn>
            <v-spacer />
            <v-btn icon size="small" color="error" variant="text" @click="confirmDelete(quiz)">
              <v-icon>mdi-delete</v-icon>
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    <!-- Create Quiz Dialog -->
    <v-dialog v-model="showCreateDialog" max-width="500">
      <v-card rounded="xl" class="pa-4">
        <v-card-title class="text-h5 font-weight-bold">Create New Quiz</v-card-title>
        <v-card-text>
          <v-text-field v-model="newQuiz.title" label="Title" variant="outlined" class="mb-3" />
          <v-textarea v-model="newQuiz.description" label="Description" variant="outlined" rows="2" class="mb-3" />
          <v-select v-model="newQuiz.difficulty" :items="difficultyItems" label="Difficulty" variant="outlined" class="mb-3" />
          <v-text-field v-model.number="newQuiz.timeLimitPerQuestion" label="Time limit per question (seconds)"
            type="number" variant="outlined" />
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showCreateDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="flat" @click="createQuiz" :loading="creating">Create</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete Confirmation -->
    <v-dialog v-model="showDeleteDialog" max-width="400">
      <v-card rounded="xl" class="pa-4">
        <v-card-title class="text-h6">Delete Quiz?</v-card-title>
        <v-card-text>
          Are you sure you want to delete <strong>{{ quizToDelete?.title }}</strong>? This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showDeleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="flat" @click="deleteQuiz" :loading="deleting">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, onMounted, inject } from 'vue'
import { useRouter } from 'vue-router'
import { useQuizStore } from '../stores/quiz'

const router = useRouter()
const quizStore = useQuizStore()
const notify = inject('snackbar')

const showCreateDialog = ref(false)
const showDeleteDialog = ref(false)
const creating = ref(false)
const deleting = ref(false)
const quizToDelete = ref(null)

const newQuiz = ref({ title: '', description: '', difficulty: 0, timeLimitPerQuestion: 30 })
const difficultyLabel = (d) => ['Easy', 'Medium', 'Hard'][d]
const difficultyColor = (d) => ['success', 'warning', 'error'][d]
const difficultyItems = [
  { title: 'Easy', value: 0 },
  { title: 'Medium', value: 1 },
  { title: 'Hard', value: 2 },
]

onMounted(() => quizStore.fetchQuizzes())

async function createQuiz() {
  creating.value = true
  try {
    const quiz = await quizStore.createQuiz(newQuiz.value)
    showCreateDialog.value = false
    newQuiz.value = { title: '', description: '', difficulty: 0, timeLimitPerQuestion: 30 }
    notify('Quiz created!', 'success')
    router.push(`/admin/quiz/${quiz.id}`)
  } catch (e) {
    notify(e.response?.data?.error || 'Failed to create quiz', 'error')
  } finally {
    creating.value = false
  }
}

function confirmDelete(quiz) {
  quizToDelete.value = quiz
  showDeleteDialog.value = true
}

async function deleteQuiz() {
  deleting.value = true
  try {
    await quizStore.deleteQuiz(quizToDelete.value.id)
    showDeleteDialog.value = false
    notify('Quiz deleted', 'info')
  } catch (e) {
    notify(e.response?.data?.error || 'Failed to delete quiz', 'error')
  } finally {
    deleting.value = false
  }
}
</script>
