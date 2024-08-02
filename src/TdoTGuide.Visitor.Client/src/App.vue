<script setup lang="ts">
import { ref } from 'vue'
import uiFetch from './UIFetch'
import ErrorWithRetry from './components/ErrorWithRetry.vue'
import LoadingBar from './components/LoadingBar.vue'
import ProjectList from './components/ProjectList.vue'
import type { Dto } from './Types'

const projectList = ref<Dto.ProjectList>()
const isLoadingProjects = ref(false)
const hasLoadingProjectsFailed = ref(false)
const loadProjects = async () => {
  const fetchResult = await uiFetch(isLoadingProjects, hasLoadingProjectsFailed, "/api/projects")
  if (fetchResult.succeeded) {
    projectList.value = await fetchResult.response.json()
  }
}
loadProjects()
</script>

<template>
  <header class="p-8 bg-htlvb flex items-center gap-4">
    <img alt="HTLVB Logo" class="logo w-[90px] md:w-[120px]" src="./assets/logo.svg" />
    <h1 class="text-2xl md:text-5xl text-white">TdoT@HTLVB</h1>
  </header>

  <main class="dark:bg-stone-800 dark:text-white">
    <div class="max-w-screen-lg mx-auto">
      <LoadingBar v-if="isLoadingProjects" class="p-8" />
      <ErrorWithRetry v-else-if="hasLoadingProjectsFailed" @retry="loadProjects">😱 Fehler beim Laden der Angebote.</ErrorWithRetry>
      <ProjectList v-else-if="projectList !== undefined" :projects="projectList.projects" :departments="projectList.departments" class="p-4" />
    </div>
  </main>
</template>
