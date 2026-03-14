<template>
  <v-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" max-width="440" persistent>
    <v-card class="pa-4" rounded="xl">
      <v-card-title class="text-center text-h5 font-weight-bold mb-2">
        {{ isLogin ? 'Welcome Back!' : 'Create Account' }}
      </v-card-title>

      <v-tabs v-model="tab" color="primary" grow class="mb-4">
        <v-tab value="login">Sign In</v-tab>
        <v-tab value="register">Sign Up</v-tab>
      </v-tabs>

      <v-window v-model="tab">
        <v-window-item value="login">
          <v-form @submit.prevent="handleLogin" ref="loginForm">
            <v-text-field v-model="loginData.email" label="Email" type="email"
              prepend-inner-icon="mdi-email" variant="outlined" density="comfortable"
              :rules="[rules.required, rules.email]" class="mb-2" />
            <v-text-field v-model="loginData.password" label="Password"
              :type="showPassword ? 'text' : 'password'"
              prepend-inner-icon="mdi-lock"
              :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
              @click:append-inner="showPassword = !showPassword"
              variant="outlined" density="comfortable"
              :rules="[rules.required]" class="mb-2" />
            <v-btn type="submit" color="primary" block size="large" :loading="loading" rounded="lg">
              Sign In
            </v-btn>
          </v-form>
        </v-window-item>

        <v-window-item value="register">
          <v-form @submit.prevent="handleRegister" ref="registerForm">
            <v-text-field v-model="registerData.username" label="Username"
              prepend-inner-icon="mdi-account" variant="outlined" density="comfortable"
              :rules="[rules.required, rules.minLength(3)]" class="mb-2" />
            <v-text-field v-model="registerData.email" label="Email" type="email"
              prepend-inner-icon="mdi-email" variant="outlined" density="comfortable"
              :rules="[rules.required, rules.email]" class="mb-2" />
            <v-text-field v-model="registerData.password" label="Password"
              :type="showPassword ? 'text' : 'password'"
              prepend-inner-icon="mdi-lock"
              :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
              @click:append-inner="showPassword = !showPassword"
              variant="outlined" density="comfortable"
              :rules="[rules.required, rules.minLength(6)]" class="mb-2" />
            <v-btn type="submit" color="primary" block size="large" :loading="loading" rounded="lg">
              Create Account
            </v-btn>
          </v-form>
        </v-window-item>
      </v-window>

      <v-alert v-if="error" type="error" variant="tonal" class="mt-3" density="compact">
        {{ error }}
      </v-alert>

      <v-btn variant="text" block class="mt-3" @click="$emit('update:modelValue', false)">Cancel</v-btn>
    </v-card>
  </v-dialog>
</template>

<script setup>
import { ref, computed, inject } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'

defineProps({ modelValue: Boolean })
const emit = defineEmits(['update:modelValue'])

const auth = useAuthStore()
const router = useRouter()
const notify = inject('snackbar')

const tab = ref('login')
const isLogin = computed(() => tab.value === 'login')
const showPassword = ref(false)
const loading = ref(false)
const error = ref('')

const loginData = ref({ email: '', password: '' })
const registerData = ref({ username: '', email: '', password: '' })

const rules = {
  required: (v) => !!v || 'Required',
  email: (v) => /.+@.+\..+/.test(v) || 'Invalid email',
  minLength: (min) => (v) => (v && v.length >= min) || `Minimum ${min} characters`,
}

async function handleLogin() {
  error.value = ''
  loading.value = true
  try {
    await auth.login(loginData.value.email, loginData.value.password)
    emit('update:modelValue', false)
    notify('Welcome back!', 'success')
    router.push('/quizzes')
  } catch (e) {
    error.value = e.response?.data?.error || 'Login failed'
  } finally {
    loading.value = false
  }
}

async function handleRegister() {
  error.value = ''
  loading.value = true
  try {
    await auth.register(registerData.value.username, registerData.value.email, registerData.value.password)
    emit('update:modelValue', false)
    notify('Account created!', 'success')
    router.push('/quizzes')
  } catch (e) {
    error.value = e.response?.data?.error || 'Registration failed'
  } finally {
    loading.value = false
  }
}
</script>
