<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Dto } from '@/Types'
import ProjectListItem from './ProjectListItem.vue'
import _ from 'lodash-es'

const props = defineProps<{
  projects: Dto.Project[]
  departments: Dto.Department[]
  buildings: Dto.Building[]
}>()

const groups = computed(() => {
  if (props.projects === undefined) return []
  return _.uniq(props.projects.map(v => v.group).filter((v): v is NonNullable<typeof v> => v !== null))
})

const selectedDepartments = ref<string[]>()
const selectDepartment = (departmentId: string) => {
  if (selectedDepartments.value === undefined) {
    selectedDepartments.value = [ departmentId ]
  }
  else {
    const index = selectedDepartments.value.indexOf(departmentId)
    if (index >= 0) {
      selectedDepartments.value.splice(index, 1)
      if (selectedDepartments.value.length === 0) {
        selectedDepartments.value = undefined
      }
    }
    else {
      selectedDepartments.value.push(departmentId)
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

const filteredProjects = computed(() => {
  let result = props.projects

  if (selectedDepartments.value !== undefined) {
    result = result.filter(project =>
      project.departments.length === 0
      || _.intersection(project.departments, selectedDepartments.value).length > 0
    )
  }

  if (selectedGroup.value !== undefined) {
    result = result.filter(project => project.group === selectedGroup.value)
  }

  if (selectedBuilding.value !== undefined) {
    result = result.filter(project => project.building === selectedBuilding.value)
  }

  return result
})
</script>

<template>
  <div>
    <div class="flex flex-col items-center gap-2">
      <header class="text-center small-caps mt-8 md:mt-16 mb-4">
        <p class="text-3xl md:text-5xl leading-normal">Tage der offenen Tür</p>
        <p class="text-xl md:text-3xl leading-normal">Fr. 22.11.2024 13:00 - 17:00</p>
        <p class="text-xl md:text-3xl leading-normal">Sa. 23.11.2024 09:00 - 13:00</p>
      </header>
      <div class="border border-white border-2 p-4 mb-4 text-xl md:text-3xl leading-normal text-center small-caps">
        <p>Gestalte deinen eigenen Rundgang</p>
        <p>&ndash; Suche dir aus, was dir gefällt &ndash;</p>
      </div>
      <div class="mb-4">
        <img src="@/assets/logo.svg" width="250">
      </div>
      <p class="text-2xl mb-2 text-center">Für welche Abteilungen interessierst du dich?</p>
      <div class="flex flex-row flex-wrap justify-center gap-2 mb-4">
        <button v-for="department in departments" :key="department.id"
          @click="() => selectDepartment(department.id)"
          class="button text-white"
          :style="{ 'background-color': (selectedDepartments === undefined || selectedDepartments.indexOf(department.id) >= 0 ? department.color : undefined) }">{{ department.longName }}</button>
      </div>
      <p class="text-2xl mb-2 text-center">Für welches Gebäude interessierst du dich?</p>
      <div class="flex flex-row flex-wrap justify-center gap-2 mb-4">
        <button v-for="building in buildings" :key="building.id"
          @click="() => selectBuilding(building.id)"
          class="button text-white"
          :class="{ 'button-htlvb-selected': selectedBuilding === building.id }">{{ building.name }}</button>
      </div>
      <p class="text-2xl mb-2 text-center">Für welche Projekte interessierst du dich?</p>
      <div class="flex flex-row flex-wrap justify-center gap-2 mb-4">
        <button v-for="group in groups" :key="group"
          @click="() => selectGroup(group)"
          class="button text-white"
          :class="{ 'button-htlvb-selected': selectedGroup === group }">{{ group }}</button>
      </div>
    </div>
    <div class="flex flex-col gap-4 mt-4">
      <span v-if="filteredProjects.length === 0" class="self-center">
        😥 Keine Angebote gefunden
      </span>
      <ProjectListItem v-for="project in filteredProjects" :key="JSON.stringify(project)"
        :project="project"
        :departments="departments"
        :buildings="buildings" />
    </div>
  </div>
</template>