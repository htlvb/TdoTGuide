<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Dto } from '@/Types'
import uiFetch from '@/UIFetch'
import { sortBy } from 'lodash-es'
import PrintFolderProjectList from './PrintFolderProjectList.vue'

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

const projectGroups = computed(() => {
  if (projectList.value === undefined) return
  return {
    theorie: {
      name: projectList.value.buildings.find(v => v.id === '1')?.name || '',
      ug: sortBy(projectList.value.projects.filter(v => v.building === '1' && v.floor === 'Untergeschoss'), v => v.title),
      eg: sortBy(projectList.value.projects.filter(v => v.building === '1' && v.floor === 'Erdgeschoss'), v => v.title),
      og: sortBy(projectList.value.projects.filter(v => v.building === '1' && v.floor !== null && /\d+\.\s*Stock/.test(v.floor)), v => v.title)
    },
    werkstaette: {
      name: projectList.value.buildings.find(v => v.id === '3')?.name || '',
      projects: sortBy(projectList.value.projects.filter(v => v.building === '3'), v => v.title)
    },
    labor: {
      name: projectList.value.buildings.find(v => v.id === '2')?.name || '',
      projects: sortBy(projectList.value.projects.filter(v => v.building === '2'), v => v.title)
    }
  }
})
</script>

<template>
  <section class="grid grid-cols-3 page">
    <div class="flex flex-col gap-4 p-4">
      <div class="text-center text-3xl small-caps">{{ projectGroups?.werkstaette.name }}</div>
      <PrintFolderProjectList v-if="projectGroups !== undefined" :projects="projectGroups.werkstaette.projects"
        class="flex-grow flex flex-col justify-center gap-4" />
    </div>
    <div class="flex flex-col gap-4 p-4">
      <div class="text-center text-3xl small-caps">{{ projectGroups?.labor.name }}</div>
      <PrintFolderProjectList v-if="projectGroups !== undefined" :projects="projectGroups.labor.projects"
        class="flex-grow flex flex-col justify-center gap-4" />
    </div>
    <div>
      <div class="flex flex-col items-center gap-4">
        <div class="text-center small-caps mt-4">
          <p class="text-4xl leading-relaxed">Tage der offenen Tür</p>
        </div>
        <div>
          <img src="@/assets/logo.svg" width="228" height="96">
        </div>
        <div class="px-4 py-2 border border-white border-2 inline-block text-3xl leading-normal text-center small-caps">
          <p>Gestalte deinen<br />eigenen<br />Rundgang -</p>
          <p>Suche dir aus,<br />was dir gefällt</p>
        </div>
        <div class="text-3xl leading-normal text-center small-caps">
          <p>Jetzt geht's los</p>
        </div>
        <div>
          <img src="@/assets/qr-code.svg" width="200">
        </div>
        <div class="text-3xl leading-normal text-center">
          <p>www.htlvb.at</p>
        </div>
      </div>
    </div>
  </section>
  <section class="grid grid-cols-3 page">
    <div class="flex flex-col gap-4 p-4">
      <PrintFolderProjectList v-if="projectGroups !== undefined" :projects="projectGroups.theorie.ug"
        class="flex-grow flex flex-col justify-center gap-4" />
    </div>
    <div class="flex flex-col gap-4 p-4">
      <div class="text-center text-3xl">{{ projectGroups?.theorie.name }}</div>
      <PrintFolderProjectList v-if="projectGroups !== undefined" :projects="projectGroups.theorie.eg"
        class="flex-grow flex flex-col justify-center gap-4" />
      <div class="text-center text-3xl">www.htlvb.at</div>
    </div>
    <div class="flex flex-col gap-4 p-4">
      <PrintFolderProjectList v-if="projectGroups !== undefined" :projects="projectGroups.theorie.og"
        class="flex-grow flex flex-col justify-center gap-4" />
    </div>
  </section>
</template>

<style lang="css" scoped>
.page {
  width: 297mm;
  height: 210mm;
}
</style>