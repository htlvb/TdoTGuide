<script setup lang="ts">
import { computed, ref } from 'vue'
import uiFetch from './UIFetch'
import ErrorWithRetry from './components/ErrorWithRetry.vue'
import LoadingBar from './components/LoadingBar.vue'
import ProjectOverview from './components/ProjectOverview.vue'
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

const groups = computed(() => {
  if (projectList.value === undefined) return []
  return _.uniq(projectList.value.projects.map(v => v.group).filter((v): v is NonNullable<typeof v> => v !== null))
})

type ProjectFilter = {
  type: 'department',
  department: string
} | {
  type: 'group',
  group: string
} | {
  type: 'building',
  building: string
}

const initialFilter = ref<ProjectFilter>()
</script>

<template>
  <main class="bg-stone-800 text-white bg-[url('@/assets/bg.jpg')] bg-cover">
    <div class="max-w-screen-lg mx-auto">
      <ErrorWithRetry v-if="hasLoadingProjectsFailed" @retry="loadProjects">😱 Fehler beim Laden der Angebote.</ErrorWithRetry>
      <ProjectOverview v-if="initialFilter === undefined"
        :departments="projectList?.departments ?? []"
        :groups="groups"
        :buildings="projectList?.buildings ?? []"
        @select-department="v => initialFilter = { type: 'department', department: v }"
        @select-group="v => initialFilter = { type: 'group', group: v }"
        @select-building="v => initialFilter = { type: 'building', building: v }" />
      <ProjectList v-else-if="projectList !== undefined"
        :projects="projectList.projects"
        :departments="projectList.departments"
        :buildings="projectList.buildings"
        :initialSelectedDepartments="initialFilter.type === 'department' ? [initialFilter.department] : undefined"
        :initialSelectedGroup="initialFilter.type === 'group' ? initialFilter.group : undefined"
        :initialSelectedBuilding="initialFilter.type === 'building' ? initialFilter.building : undefined"
        class="p-4" />
    </div>
  </main>
</template>
