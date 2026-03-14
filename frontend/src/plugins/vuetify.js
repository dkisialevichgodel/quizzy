import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

export default createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'dark',
    themes: {
      dark: {
        colors: {
          primary: '#7C4DFF',
          secondary: '#448AFF',
          accent: '#FF4081',
          success: '#66BB6A',
          warning: '#FFA726',
          error: '#EF5350',
          background: '#121212',
          surface: '#1E1E2E',
        },
      },
      light: {
        colors: {
          primary: '#7C4DFF',
          secondary: '#448AFF',
          accent: '#FF4081',
          success: '#66BB6A',
          warning: '#FFA726',
          error: '#EF5350',
        },
      },
    },
  },
})
