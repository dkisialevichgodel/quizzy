<template>
  <v-app>
    <v-app-bar color="surface" elevation="2" density="compact">
      <v-app-bar-title>
        <router-link to="/" class="text-decoration-none">
          <span class="text-primary font-weight-bold text-h5">⚡ Quizzy</span>
        </router-link>
      </v-app-bar-title>

      <template v-slot:append>
        <template v-if="auth.isAuthenticated">
          <v-btn variant="text" to="/quizzes" class="mr-1">Quizzes</v-btn>
          <v-btn variant="text" to="/history" class="mr-1">History</v-btn>
          <v-btn v-if="auth.isAdmin" variant="text" to="/admin" class="mr-1" color="accent">Admin</v-btn>
          <v-menu>
            <template v-slot:activator="{ props }">
              <v-btn icon v-bind="props">
                <v-avatar color="primary" size="32">
                  <span class="text-body-2">{{ auth.user?.username?.charAt(0)?.toUpperCase() }}</span>
                </v-avatar>
              </v-btn>
            </template>
            <v-list density="compact">
              <v-list-item>
                <v-list-item-title class="font-weight-bold">{{ auth.user?.username }}</v-list-item-title>
                <v-list-item-subtitle>{{ auth.user?.email }}</v-list-item-subtitle>
              </v-list-item>
              <v-divider />
              <v-list-item @click="handleLogout" prepend-icon="mdi-logout">
                <v-list-item-title>Logout</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
        </template>
        <template v-else>
          <v-btn variant="outlined" color="primary" @click="showAuth = true">Sign In</v-btn>
        </template>
      </template>
    </v-app-bar>

    <v-main>
      <router-view />
    </v-main>

    <AuthModal v-model="showAuth" />

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" :timeout="3000" location="bottom right">
      {{ snackbar.text }}
    </v-snackbar>
  </v-app>
</template>

<script setup>
import { ref, provide, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from './stores/auth'
import AuthModal from './components/AuthModal.vue'

const auth = useAuthStore()
const route = useRoute()
const showAuth = ref(false)
const snackbar = ref({ show: false, text: '', color: 'success' })

provide('snackbar', (text, color = 'success') => {
  snackbar.value = { show: true, text, color }
})

onMounted(() => {
  if (auth.isAuthenticated) auth.fetchMe()
})

watch(() => route.query.login, (val) => {
  if (val) showAuth.value = true
})

function handleLogout() {
  auth.logout()
  snackbar.value = { show: true, text: 'Logged out successfully', color: 'info' }
}
</script>
