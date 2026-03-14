import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { shallowMount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import AuthModal from '../../components/AuthModal.vue'
import { useAuthStore } from '../../stores/auth'

// Mock router
const mockRouterPush = vi.fn()
vi.mock('vue-router', async () => {
  const actual = await vi.importActual('vue-router')
  return {
    ...actual,
    useRouter: () => ({
      push: mockRouterPush
    })
  }
})

// Mock API
vi.mock('../../api')

describe('AuthModal Component', () => {
  let wrapper
  let pinia

  const mountComponent = (props = {}) => {
    const snackbarMock = vi.fn()
    
    return shallowMount(AuthModal, {
      props: {
        modelValue: false,
        ...props
      },
      global: {
        plugins: [pinia],
        provide: {
          snackbar: snackbarMock
        }
      }
    })
  }

  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)
    vi.clearAllMocks()
    mockRouterPush.mockClear()
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('Form Validation', () => {
    it('should have required rule', () => {
      wrapper = mountComponent({ modelValue: true })
      const rule = wrapper.vm.rules.required
      
      expect(rule('test')).toBe(true)
      expect(rule('')).toBe('Required')
      expect(rule(null)).toBe('Required')
    })

    it('should have email validation rule', () => {
      wrapper = mountComponent({ modelValue: true })
      const rule = wrapper.vm.rules.email
      
      expect(rule('test@example.com')).toBe(true)
      expect(rule('invalid-email')).toBe('Invalid email')
    })

    it('should have minLength validation rule', () => {
      wrapper = mountComponent({ modelValue: true })
      const rule = wrapper.vm.rules.minLength(3)
      
      expect(rule('test')).toBe(true)
      expect(rule('ab')).toBe('Minimum 3 characters')
    })
  })

  describe('Login Action', () => {
    it('should call auth.login with correct credentials', async () => {
      const authStore = useAuthStore()
      authStore.login = vi.fn().mockResolvedValue({ token: 'test-token' })

      wrapper = mountComponent({ modelValue: true })
      wrapper.vm.loginData = { email: 'test@test.com', password: 'password123' }
      
      await wrapper.vm.handleLogin()

      expect(authStore.login).toHaveBeenCalledWith('test@test.com', 'password123')
    })

    it('should emit update:modelValue false on successful login', async () => {
      const authStore = useAuthStore()
      authStore.login = vi.fn().mockResolvedValue({ token: 'test-token' })

      wrapper = mountComponent({ modelValue: true })
      wrapper.vm.loginData = { email: 'test@test.com', password: 'password123' }
      
      await wrapper.vm.handleLogin()

      expect(wrapper.emitted('update:modelValue')).toBeTruthy()
      expect(wrapper.emitted('update:modelValue')[0]).toEqual([false])
    })

    it('should display error message on login failure', async () => {
      const authStore = useAuthStore()
      const errorMessage = 'Invalid credentials'
      authStore.login = vi.fn().mockRejectedValue({
        response: { data: { error: errorMessage } }
      })

      wrapper = mountComponent({ modelValue: true })
      wrapper.vm.loginData = { email: 'wrong@test.com', password: 'wrong' }
      
      await wrapper.vm.handleLogin()

      expect(wrapper.vm.error).toBe(errorMessage)
    })
  })

  describe('Register Action', () => {
    it('should call auth.register with correct data', async () => {
      const authStore = useAuthStore()
      authStore.register = vi.fn().mockResolvedValue({ token: 'test-token' })

      wrapper = mountComponent({ modelValue: true })
      wrapper.vm.registerData = { 
        username: 'newuser', 
        email: 'new@test.com', 
        password: 'password123' 
      }
      
      await wrapper.vm.handleRegister()

      expect(authStore.register).toHaveBeenCalledWith('newuser', 'new@test.com', 'password123')
    })

    it('should display error message on registration failure', async () => {
      const authStore = useAuthStore()
      const errorMessage = 'Email already exists'
      authStore.register = vi.fn().mockRejectedValue({
        response: { data: { error: errorMessage } }
      })

      wrapper = mountComponent({ modelValue: true })
      wrapper.vm.registerData = { 
        username: 'user', 
        email: 'exists@test.com', 
        password: 'pass' 
      }
      
      await wrapper.vm.handleRegister()

      expect(wrapper.vm.error).toBe(errorMessage)
    })
  })
})
