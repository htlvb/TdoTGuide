<script setup lang="ts">
import { computed, ref } from 'vue'
import uiFetch from './UIFetch'
import LoadingBar from './components/LoadingBar.vue'
import ErrorWithRetry from './components/ErrorWithRetry.vue'
import ProjectListItem from './components/ProjectListItem.vue'
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

const selectedTags = ref([] as Dto.ProjectTag[])
const toggleProjectTagFilter = (projectTag: Dto.ProjectTag) => {
  const index = selectedTags.value.indexOf(projectTag)
  if (index >= 0) {
    selectedTags.value.splice(index, 1)
  }
  else {
    selectedTags.value.push(projectTag)
  }
}

const selectedBuilding = ref<string>()
const selectBuilding = (buildingId: string) => {
  selectedBuilding.value = selectedBuilding.value === buildingId ? undefined : buildingId
}

const filteredProjects = computed(() => {
  if (projectList.value === undefined) return []

  let result = projectList.value.projects

  if (selectedTags.value.length > 0) {
    const filters = selectedTags.value.map(v => v.longName)
    result = result.filter(project => project.tags.some(projectTag => filters.includes(projectTag.longName)))
  }

  if (selectedBuilding.value !== undefined) {
    result = result.filter(project => project.building === selectedBuilding.value)
  }

  return result
})
</script>

<template>
  <main class="text-white bg-[url('@/assets/bg.jpg')] bg-[#183f7c] bg-cover bg-top h-full overflow-y-scroll">
    <div class="max-w-screen-lg mx-auto">
      <div class="flex flex-col items-center gap-6">
        <header class="flex flex-col items-center gap-6 print:[zoom:1.5]">
          <div class="text-center small-caps mt-8 md:mt-16">
            <p class="text-3xl md:text-5xl leading-normal print:leading-relaxed">Tage der offenen Tür</p>
            <p class="text-xl md:text-3xl leading-normal print:leading-relaxed">Fr. 22.11.2024 13:00 - 17:00</p>
            <p class="text-xl md:text-3xl leading-normal print:leading-relaxed">Sa. 23.11.2024 09:00 - 13:00</p>
          </div>
          <div class="border border-white border-2 p-4 text-xl md:text-3xl leading-normal text-center small-caps">
            <p>Gestalte deinen eigenen Rundgang</p>
            <p>&ndash; Suche dir aus, was dir gefällt &ndash;</p>
          </div>
          <div class="hidden print:block">
            <img src="@/assets/qr-code.svg" width="150">
          </div>
          <div>
            <img src="@/assets/logo.svg" width="250" height="106">
          </div>
        </header>
        <LoadingBar v-if="isLoadingProjects" />
        <ErrorWithRetry v-else-if="hasLoadingProjectsFailed" @retry="loadProjects">😱 Fehler beim Laden der Angebote.</ErrorWithRetry>
        <section v-else-if="projectList !== undefined" class="flex flex-col gap-6 print:hidden">
          <p class="text-2xl md:text-3xl text-center">&mdash; Triff deine Auswahl &mdash;</p>
          <section class="flex flex-col items-center gap-2 animation-fade-in">
            <div class="flex flex-col gap-2">
              <div v-for="projectTagGroup in projectList.projectTags" :key="JSON.stringify(projectTagGroup)"
                class="flex flex-row flex-wrap justify-center gap-2">
                <button v-for="projectTag in projectTagGroup" :key="projectTag.longName"
                  @click="() => toggleProjectTagFilter(projectTag)"
                  class="button text-white"
                  :style="{ 'background': (selectedTags.length === 0 || selectedTags.indexOf(projectTag) >= 0 ? projectTag.color : undefined) }">{{ projectTag.longName }}</button>
              </div>
            </div>
          </section>
          <section class="flex flex-col items-center gap-2 animation-fade-in ![animation-delay:1s]">
            <p class="text-lg md:text-2xl text-center">Optional: Nur Angebote im ausgewählten Gebäude anzeigen</p>
            <div class="flex flex-row flex-wrap justify-center gap-2">
              <button v-for="building in projectList.buildings" :key="building.id"
                @click="() => selectBuilding(building.id)"
                class="button text-white"
                :class="{ 'button-htlvb-selected': selectedBuilding === building.id }">{{ building.name }}</button>
            </div>
          </section>
          <section class="self-stretch flex flex-col gap-4 mt-4 animation-fade-in ![animation-delay:3s] print:hidden">
            <span v-if="filteredProjects.length === 0" class="self-center">
              😥 Keine Angebote gefunden
            </span>
            <ProjectListItem v-for="project in filteredProjects" :key="JSON.stringify(project)"
              :project="project"
              :buildings="projectList.buildings" />
          </section>
        </section>
      </div>
    </div>
  </main>
</template>
