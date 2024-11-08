import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/', component: () => import('@/views/HomeView.vue') },
  {
    path: '/print',
    children: [
      { path: '', component: () => import('@/views/PrintOverviewView.vue') },
      { path: 'flyer', component: () => import('@/views/PrintFlyerView.vue') },
      { path: 'folder', component: () => import('@/views/PrintFolderView.vue') },
      { path: 'building-overview/:buildingId?', component: () => import('@/views/BuildingsOverviewView.vue') }
    ]
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})