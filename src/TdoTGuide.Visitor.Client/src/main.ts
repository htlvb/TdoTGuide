import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'

import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faAnglesDown, faAnglesUp } from '@fortawesome/free-solid-svg-icons'

library.add(faAnglesDown, faAnglesUp)

createApp(App)
    .component('font-awesome-icon', FontAwesomeIcon)
    .mount('#app')
