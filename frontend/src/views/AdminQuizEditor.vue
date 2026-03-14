<template>
  <v-container class="py-8" max-width="900">
    <div v-if="loading" class="text-center py-16">
      <v-progress-circular indeterminate size="64" color="primary" />
    </div>

    <template v-else-if="quiz">
      <!-- Quiz Header -->
      <div class="d-flex align-center mb-6">
        <v-btn icon variant="text" @click="router.push('/admin')" class="mr-2">
          <v-icon>mdi-arrow-left</v-icon>
        </v-btn>
        <div class="flex-grow-1">
          <h1 class="text-h5 font-weight-bold">{{ quiz.title }}</h1>
          <div class="text-medium-emphasis">
            <v-chip :color="difficultyColor(quiz.difficulty)" size="x-small" variant="flat" class="mr-1">
              {{ difficultyLabel(quiz.difficulty) }}
            </v-chip>
            {{ quiz.questions.length }} questions
          </div>
        </div>
        <v-btn color="primary" variant="flat" rounded="lg" @click="openQuestionDialog()">
          <v-icon start>mdi-plus</v-icon>
          Add Question
        </v-btn>
      </div>

      <!-- Questions List -->
      <v-card v-for="(question, i) in quiz.questions" :key="question.id" rounded="xl" elevation="1" class="mb-3">
        <v-card-text class="pa-4">
          <div class="d-flex justify-space-between align-start">
            <div class="flex-grow-1">
              <div class="d-flex align-center gap-2 mb-1">
                <v-chip size="x-small" variant="flat" color="primary">Q{{ i + 1 }}</v-chip>
                <v-chip size="x-small" :color="question.type === 0 ? 'secondary' : 'accent'" variant="flat">
                  {{ question.type === 0 ? 'Multiple Choice' : 'Open Text' }}
                </v-chip>
                <v-chip size="x-small" variant="outlined">{{ question.points }} pts</v-chip>
              </div>
              <p class="font-weight-medium mt-1">{{ question.text }}</p>

              <!-- Answer Options -->
              <div v-if="question.type === 0" class="mt-2">
                <v-chip v-for="opt in question.answerOptions" :key="opt.id" size="small" class="mr-1 mb-1"
                  :color="opt.isCorrect ? 'success' : ''" :variant="opt.isCorrect ? 'flat' : 'outlined'">
                  {{ opt.isCorrect ? '✓' : '' }} {{ opt.text }}
                </v-chip>
              </div>
              <div v-else class="mt-2">
                <v-chip v-for="ans in question.openTextAnswers" :key="ans.id" size="small" color="accent" variant="tonal" class="mr-1">
                  {{ ans.text }} (≥{{ (ans.similarityThreshold * 100).toFixed(0) }}%)
                </v-chip>
              </div>
            </div>

            <div class="d-flex">
              <v-btn icon size="small" variant="text" @click="openQuestionDialog(question)">
                <v-icon>mdi-pencil</v-icon>
              </v-btn>
              <v-btn icon size="small" variant="text" color="error" @click="deleteQuestion(question.id)">
                <v-icon>mdi-delete</v-icon>
              </v-btn>
            </div>
          </div>
        </v-card-text>
      </v-card>

      <v-card v-if="!quiz.questions.length" rounded="xl" variant="outlined" class="text-center pa-12">
        <v-icon size="48" color="grey" class="mb-3">mdi-help-circle-outline</v-icon>
        <p class="text-h6 text-medium-emphasis">No questions yet</p>
        <v-btn color="primary" variant="flat" class="mt-3" @click="openQuestionDialog()">Add First Question</v-btn>
      </v-card>
    </template>

    <!-- Question Dialog -->
    <v-dialog v-model="showQuestionDialog" max-width="700" scrollable>
      <v-card rounded="xl" class="pa-4">
        <v-card-title class="text-h5 font-weight-bold">
          {{ editingQuestion ? 'Edit Question' : 'Add Question' }}
        </v-card-title>
        <v-card-text>
          <v-textarea v-model="questionForm.text" label="Question text" variant="outlined" rows="2" class="mb-3" />

          <v-row class="mb-3">
            <v-col cols="6">
              <v-select v-model="questionForm.type" :items="questionTypes" label="Type" variant="outlined" />
            </v-col>
            <v-col cols="3">
              <v-text-field v-model.number="questionForm.points" label="Points" type="number" variant="outlined" />
            </v-col>
            <v-col cols="3">
              <v-text-field v-model.number="questionForm.orderIndex" label="Order" type="number" variant="outlined" />
            </v-col>
          </v-row>

          <!-- Multiple Choice Options -->
          <template v-if="questionForm.type === 0">
            <h4 class="mb-2">Answer Options</h4>
            <div v-for="(opt, i) in questionForm.answerOptions" :key="i" class="d-flex align-center gap-2 mb-2">
              <v-text-field v-model="opt.text" :label="`Option ${i + 1}`" variant="outlined" density="compact"
                hide-details class="flex-grow-1" />
              <v-checkbox v-model="opt.isCorrect" hide-details density="compact" color="success"
                @update:model-value="setCorrect(i)" />
              <v-btn icon size="x-small" variant="text" color="error" @click="questionForm.answerOptions.splice(i, 1)">
                <v-icon>mdi-close</v-icon>
              </v-btn>
            </div>
            <v-btn variant="text" size="small" @click="questionForm.answerOptions.push({ text: '', isCorrect: false, mediaId: null })">
              <v-icon start>mdi-plus</v-icon> Add Option
            </v-btn>
          </template>

          <!-- Open Text Answers -->
          <template v-else>
            <h4 class="mb-2">Reference Answers</h4>
            <div v-for="(ans, i) in questionForm.openTextAnswers" :key="i" class="d-flex align-center gap-2 mb-2">
              <v-text-field v-model="ans.text" :label="`Reference ${i + 1}`" variant="outlined" density="compact"
                hide-details class="flex-grow-1" />
              <v-text-field v-model.number="ans.similarityThreshold" label="Threshold" type="number"
                step="0.05" min="0" max="1" variant="outlined" density="compact" hide-details style="max-width: 100px" />
              <v-btn icon size="x-small" variant="text" color="error" @click="questionForm.openTextAnswers.splice(i, 1)">
                <v-icon>mdi-close</v-icon>
              </v-btn>
            </div>
            <v-btn variant="text" size="small" @click="questionForm.openTextAnswers.push({ text: '', similarityThreshold: 0.7 })">
              <v-icon start>mdi-plus</v-icon> Add Reference
            </v-btn>
          </template>

          <!-- Media Upload -->
          <div class="mt-4">
            <v-file-input v-model="mediaFile" label="Attach media (image/audio)" variant="outlined"
              accept="image/*,audio/*" prepend-icon="mdi-paperclip" density="compact" />
          </div>
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="showQuestionDialog = false">Cancel</v-btn>
          <v-btn color="primary" variant="flat" @click="saveQuestion" :loading="saving">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, onMounted, inject } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '../api'

const route = useRoute()
const router = useRouter()
const notify = inject('snackbar')

const quizId = parseInt(route.params.id)
const loading = ref(true)
const saving = ref(false)
const quiz = ref(null)
const showQuestionDialog = ref(false)
const editingQuestion = ref(null)
const mediaFile = ref(null)

const difficultyLabel = (d) => ['Easy', 'Medium', 'Hard'][d]
const difficultyColor = (d) => ['success', 'warning', 'error'][d]
const questionTypes = [
  { title: 'Multiple Choice', value: 0 },
  { title: 'Open Text', value: 1 },
]

const questionForm = ref(createEmptyForm())

function createEmptyForm() {
  return {
    text: '',
    type: 0,
    points: 10,
    orderIndex: 0,
    mediaId: null,
    timeLimitOverride: null,
    answerOptions: [
      { text: '', isCorrect: true, mediaId: null },
      { text: '', isCorrect: false, mediaId: null },
      { text: '', isCorrect: false, mediaId: null },
      { text: '', isCorrect: false, mediaId: null },
    ],
    openTextAnswers: [{ text: '', similarityThreshold: 0.7 }],
  }
}

function setCorrect(index) {
  questionForm.value.answerOptions.forEach((opt, i) => {
    opt.isCorrect = i === index
  })
}

async function loadQuiz() {
  loading.value = true
  try {
    const { data } = await api.get(`/quizzes/${quizId}`)
    quiz.value = data
  } catch {
    notify('Failed to load quiz', 'error')
    router.push('/admin')
  } finally {
    loading.value = false
  }
}

function openQuestionDialog(question = null) {
  editingQuestion.value = question
  if (question) {
    questionForm.value = {
      text: question.text,
      type: question.type,
      points: question.points,
      orderIndex: question.orderIndex,
      mediaId: question.mediaId,
      timeLimitOverride: question.timeLimitOverride,
      answerOptions: question.answerOptions?.map((a) => ({ text: a.text, isCorrect: a.isCorrect, mediaId: a.mediaId })) || [],
      openTextAnswers: question.openTextAnswers?.map((a) => ({ text: a.text, similarityThreshold: a.similarityThreshold })) || [],
    }
  } else {
    questionForm.value = createEmptyForm()
    questionForm.value.orderIndex = quiz.value?.questions?.length || 0
  }
  mediaFile.value = null
  showQuestionDialog.value = true
}

async function saveQuestion() {
  saving.value = true
  try {
    let mediaId = questionForm.value.mediaId

    // Upload media if provided
    if (mediaFile.value) {
      const formData = new FormData()
      formData.append('file', mediaFile.value)
      const { data } = await api.post('/media', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      mediaId = data.id
    }

    const payload = {
      ...questionForm.value,
      quizId: quizId,
      mediaId,
    }

    if (editingQuestion.value) {
      await api.put(`/questions/${editingQuestion.value.id}`, payload)
      notify('Question updated!', 'success')
    } else {
      await api.post('/questions', payload)
      notify('Question added!', 'success')
    }

    showQuestionDialog.value = false
    await loadQuiz()
  } catch (e) {
    notify(e.response?.data?.error || 'Failed to save question', 'error')
  } finally {
    saving.value = false
  }
}

async function deleteQuestion(id) {
  try {
    await api.delete(`/questions/${id}`)
    notify('Question deleted', 'info')
    await loadQuiz()
  } catch (e) {
    notify(e.response?.data?.error || 'Failed to delete question', 'error')
  }
}

onMounted(loadQuiz)
</script>
