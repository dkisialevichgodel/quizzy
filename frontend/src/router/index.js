import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/HomePage.vue'),
  },
  {
    path: '/quizzes',
    name: 'Quizzes',
    component: () => import('../views/QuizList.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/quiz/:id/play',
    name: 'QuizPlay',
    component: () => import('../views/QuizPlay.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/quiz/:quizId/result/:attemptId',
    name: 'QuizResult',
    component: () => import('../views/QuizResult.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/history',
    name: 'History',
    component: () => import('../views/HistoryPage.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/admin',
    name: 'Admin',
    component: () => import('../views/AdminDashboard.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
  },
  {
    path: '/admin/quiz/:id',
    name: 'AdminQuizEditor',
    component: () => import('../views/AdminQuizEditor.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  const user = JSON.parse(localStorage.getItem('user') || 'null')

  if (to.meta.requiresAuth && !token) {
    next({ name: 'Home', query: { login: '1' } })
  } else if (to.meta.requiresAdmin && user?.role !== 'Admin') {
    next({ name: 'Home' })
  } else {
    next()
  }
})

export default router
