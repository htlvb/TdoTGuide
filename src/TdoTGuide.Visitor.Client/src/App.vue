<script setup lang="ts">
import { computed, ref } from 'vue'
import uiFetch from './UIFetch'
import LoadingBar from './components/LoadingBar.vue'
import ErrorWithRetry from './components/ErrorWithRetry.vue'
import ProjectListItem from './components/ProjectListItem.vue'
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
  return _.uniq(projectList.value.projects.flatMap(v => v.groups))
})

type DepartmentFilter = ({ type: 'department' } & Dto.Department) | { type: 'all-departments', id: 'all-departments', longName: string, color: string }
const departments = computed(() : DepartmentFilter[] => {
  if (projectList.value === undefined) return []
  return [{ type: 'all-departments', id: 'all-departments', longName: 'Allgemein', color: '#0A1B35' }, ...projectList.value.departments.map((v) : DepartmentFilter => ({ ...v, type: 'department' }))]
})

const selectedDepartments = ref<DepartmentFilter[]>()
const selectDepartment = (department: DepartmentFilter) => {
  if (selectedDepartments.value === undefined) {
    selectedDepartments.value = [ department ]
  }
  else {
    const index = selectedDepartments.value.indexOf(department)
    if (index >= 0) {
      selectedDepartments.value.splice(index, 1)
      if (selectedDepartments.value.length === 0) {
        selectedDepartments.value = undefined
      }
    }
    else {
      selectedDepartments.value.push(department)
    }
  }
}

const selectedBuilding = ref<string>()
const selectBuilding = (buildingId: string) => {
  selectedBuilding.value = selectedBuilding.value === buildingId ? undefined : buildingId
}

const selectedGroup = ref<string>()
const selectGroup = (groupId: string) => {
  selectedGroup.value = selectedGroup.value === groupId ? undefined : groupId
}

const departmentFiltersMatchProject = (project: Dto.Project, filters: DepartmentFilter[]) => {
  if (projectList.value === undefined) return false
  const numberOfDepartments = projectList.value.departments.length
  if (filters.some(v => v.type === 'all-departments') && (project.departments.length === 0 || project.departments.length === numberOfDepartments)) {
    return true
  }
  if (project.departments.length === 0 || project.departments.length === numberOfDepartments) {
    return false
  }
  const departmentFilters = filters.filter(v => v.type === 'department').map(v => v.id)
  return _.intersection(project.departments, departmentFilters).length > 0

}

const filteredProjects = computed(() => {
  if (projectList.value === undefined) return []

  let result = projectList.value.projects

  if (selectedDepartments.value !== undefined) {
    const filters = selectedDepartments.value
    result = result.filter(project => departmentFiltersMatchProject(project, filters))
  }

  if (selectedGroup.value !== undefined) {
    const selectedGroupName = selectedGroup.value
    result = result.filter(project => project.groups.includes(selectedGroupName))
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
          <p class="text-2xl md:text-3xl text-center">&mdash; Wähle deine Kombination &mdash;</p>
          <section class="flex flex-col items-center gap-2 animation-fade-in">
            <p class="text-lg md:text-2xl text-center">Für welche Abteilungen interessierst du dich?</p>
            <div class="flex flex-row flex-wrap justify-center gap-2">
              <button v-for="department in departments" :key="department.id"
                @click="() => selectDepartment(department)"
                class="button text-white"
                :style="{ 'background-color': (selectedDepartments === undefined || selectedDepartments.indexOf(department) >= 0 ? department.color : undefined) }">{{ department.longName }}</button>
            </div>
          </section>
          <section class="flex flex-col items-center gap-2 animation-fade-in ![animation-delay:1s]">
            <p class="text-lg md:text-2xl text-center">Für welches Gebäude interessierst du dich?</p>
            <div class="flex flex-row flex-wrap justify-center gap-2">
              <button v-for="building in projectList.buildings" :key="building.id"
                @click="() => selectBuilding(building.id)"
                class="button text-white"
                :class="{ 'button-htlvb-selected': selectedBuilding === building.id }">{{ building.name }}</button>
            </div>
          </section>
          <section class="flex flex-col items-center gap-2 animation-fade-in ![animation-delay:2s]">
            <p class="text-lg md:text-2xl text-center">Für welche Projekte interessierst du dich?</p>
            <div class="flex flex-row flex-wrap justify-center gap-2">
              <button v-for="group in groups" :key="group"
                @click="() => selectGroup(group)"
                class="button text-white"
                :class="{ 'button-htlvb-selected': selectedGroup === group }">{{ group }}</button>
            </div>
          </section>
          <section class="self-stretch flex flex-col gap-4 mt-4 animation-fade-in ![animation-delay:3s] print:hidden">
            <span v-if="filteredProjects.length === 0" class="self-center">
              😥 Keine Angebote gefunden
            </span>
            <ProjectListItem v-for="project in filteredProjects" :key="JSON.stringify(project)"
              :project="project"
              :departments="projectList.departments"
              :buildings="projectList.buildings" />
          </section>
        </section>
      </div>
    </div>
  </main>
</template>
