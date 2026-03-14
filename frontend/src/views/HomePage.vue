<template>
  <div>
    <!-- Hero Section -->
    <section class="hero-section">
      <v-container class="text-center py-16">
        <v-row justify="center">
          <v-col cols="12" md="8" lg="6">
            <h1 class="text-h2 font-weight-black mb-4">
              Challenge Your <span class="text-primary">Knowledge</span>
            </h1>
            <p class="text-h6 text-medium-emphasis mb-8">
              Test yourself across different difficulty levels, compete with others,
              and climb the leaderboard. Are you ready?
            </p>
            <v-btn color="primary" size="x-large" rounded="pill" @click="handleStartGame" elevation="8"
              class="px-12 py-3 text-h6">
              <v-icon start>mdi-play-circle</v-icon>
              Start Playing
            </v-btn>
          </v-col>
        </v-row>

        <!-- Stats -->
        <v-row justify="center" class="mt-12">
          <v-col cols="4" md="2" v-for="stat in stats" :key="stat.label">
            <div class="text-h4 font-weight-bold text-primary">{{ stat.value }}</div>
            <div class="text-caption text-medium-emphasis">{{ stat.label }}</div>
          </v-col>
        </v-row>
      </v-container>
    </section>

    <!-- Difficulty Cards -->
    <v-container class="py-8">
      <h2 class="text-h4 font-weight-bold text-center mb-8">Choose Your Challenge</h2>
      <v-row justify="center">
        <v-col cols="12" sm="4" v-for="level in difficultyLevels" :key="level.value">
          <v-card :color="level.color" variant="tonal" rounded="xl" class="text-center pa-6 card-hover" elevation="0">
            <v-icon :icon="level.icon" size="48" :color="level.color" class="mb-3" />
            <h3 class="text-h5 font-weight-bold mb-2">{{ level.label }}</h3>
            <p class="text-body-2 text-medium-emphasis">{{ level.description }}</p>
            <v-chip class="mt-3" :color="level.color" variant="flat" size="small">
              {{ level.time }}s per question
            </v-chip>
          </v-card>
        </v-col>
      </v-row>
    </v-container>

    <!-- Leaderboards -->
    <v-container class="py-8">
      <h2 class="text-h4 font-weight-bold text-center mb-8">
        <v-icon color="warning" class="mr-2">mdi-trophy</v-icon>
        Leaderboards
      </h2>

      <v-tabs v-model="leaderboardTab" color="primary" centered grow class="mb-6">
        <v-tab v-for="level in difficultyLevels" :key="level.value" :value="level.value">
          <v-icon :icon="level.icon" start :color="level.color" />
          {{ level.label }}
        </v-tab>
      </v-tabs>

      <v-window v-model="leaderboardTab">
        <v-window-item v-for="level in difficultyLevels" :key="level.value" :value="level.value">
          <v-card rounded="xl" elevation="0" variant="outlined">
            <v-table v-if="leaderboard.entries[level.value]?.length" density="comfortable">
              <thead>
                <tr>
                  <th>Rank</th>
                  <th>Player</th>
                  <th>Best Score</th>
                  <th>Attempts</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="entry in leaderboard.entries[level.value]" :key="entry.userId">
                  <td>
                    <v-chip :color="entry.rank <= 3 ? ['warning', 'grey', 'deep-orange'][entry.rank - 1] : 'default'"
                      size="small" variant="flat" class="font-weight-bold">
                      {{ entry.rank <= 3 ? ['🥇', '🥈', '🥉'][entry.rank - 1] : '#' + entry.rank }}
                    </v-chip>
                  </td>
                  <td class="font-weight-medium">{{ entry.username }}</td>
                  <td>
                    <span class="text-primary font-weight-bold">{{ entry.bestScore }}</span>
                    <span class="text-medium-emphasis"> / {{ entry.maxPossibleScore }}</span>
                  </td>
                  <td>{{ entry.attemptsCount }}</td>
                </tr>
              </tbody>
            </v-table>
            <v-card-text v-else class="text-center text-medium-emphasis py-12">
              <v-icon size="48" color="grey" class="mb-3">mdi-trophy-outline</v-icon>
              <p>No scores yet. Be the first to play!</p>
            </v-card-text>
          </v-card>
        </v-window-item>
      </v-window>
    </v-container>

    <!-- Features -->
    <v-container class="py-8 pb-16">
      <v-row justify="center">
        <v-col cols="12" sm="6" md="3" v-for="feature in features" :key="feature.title">
          <v-card rounded="xl" variant="tonal" class="pa-5 text-center" color="surface" elevation="0">
            <v-icon :icon="feature.icon" size="40" color="primary" class="mb-3" />
            <h4 class="text-subtitle-1 font-weight-bold mb-1">{{ feature.title }}</h4>
            <p class="text-body-2 text-medium-emphasis">{{ feature.desc }}</p>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </div>
</template>

<script setup>
import { ref, onMounted, inject } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useLeaderboardStore } from '../stores/leaderboard'

const router = useRouter()
const auth = useAuthStore()
const leaderboard = useLeaderboardStore()
const notify = inject('snackbar')

const leaderboardTab = ref(0)

const stats = [
  { value: '3', label: 'Difficulty Levels' },
  { value: '30', label: 'Questions' },
  { value: '2', label: 'Question Types' },
]

const difficultyLevels = [
  { value: 0, label: 'Easy', color: 'success', icon: 'mdi-star-outline', description: 'Perfect for warming up your brain!', time: 30 },
  { value: 1, label: 'Medium', color: 'warning', icon: 'mdi-star-half-full', description: 'A balanced challenge for the curious mind.', time: 25 },
  { value: 2, label: 'Hard', color: 'error', icon: 'mdi-star', description: 'Only for the truly brave and knowledgeable!', time: 20 },
]

const features = [
  { icon: 'mdi-brain', title: 'Smart Scoring', desc: 'Partial credit for text answers using AI similarity' },
  { icon: 'mdi-timer', title: 'Time Bonus', desc: 'Answer faster to earn extra points' },
  { icon: 'mdi-image-multiple', title: 'Rich Media', desc: 'Questions with images and audio' },
  { icon: 'mdi-trophy', title: 'Leaderboards', desc: 'Compete and climb the rankings' },
]

onMounted(() => {
  leaderboard.fetchAll()
})

function handleStartGame() {
  if (auth.isAuthenticated) {
    router.push('/quizzes')
  } else {
    // Trigger auth modal via parent
    router.push({ query: { login: '1' } })
  }
}
</script>

<style scoped>
.hero-section {
  background: linear-gradient(135deg, rgba(124, 77, 255, 0.1) 0%, rgba(68, 138, 255, 0.05) 100%);
  min-height: 60vh;
  display: flex;
  align-items: center;
}

.card-hover {
  transition: transform 0.2s, box-shadow 0.2s;
}

.card-hover:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(124, 77, 255, 0.15);
}
</style>
