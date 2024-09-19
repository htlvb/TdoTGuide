<script setup lang="ts">
import { ref } from 'vue'
import uiFetch from './UIFetch'
import ErrorWithRetry from './components/ErrorWithRetry.vue'
import ProjectList from './components/ProjectList.vue'
import type { Dto } from './Types'
import _ from 'lodash'

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
  <main class="bg-stone-800 text-white bg-[url('@/assets/bg.jpg')] bg-cover">
    <div class="max-w-screen-lg mx-auto">
      <ErrorWithRetry v-if="hasLoadingProjectsFailed" @retry="loadProjects">😱 Fehler beim Laden der Angebote.</ErrorWithRetry>
      <ProjectList v-else-if="projectList !== undefined"
        :projects="projectList.projects"
        :departments="projectList.departments"
        :buildings="projectList.buildings"
        class="p-4" />
    </div>
  </main>
</template>
