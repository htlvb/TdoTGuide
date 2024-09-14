<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Dto } from '@/Types'
import ProjectListItem from './ProjectListItem.vue'
import _ from 'lodash-es'

const props = defineProps<{
  projects: Dto.Project[]
  departments: Dto.Department[]
  buildings: Dto.Building[]
  initialSelectedDepartments?: Dto.Department['id'][]
  initialSelectedGroup?: string
  initialSelectedBuilding?: Dto.Building['id']
}>()

const groups = computed(() => {
  if (props.projects === undefined) return []
  return _.uniq(props.projects.map(v => v.group).filter((v): v is NonNullable<typeof v> => v !== null))
})

const selectedDepartments = ref(props.initialSelectedDepartments)
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

const selectedBuilding = ref(props.initialSelectedBuilding)
const selectBuilding = (buildingId: string) => {
  selectedBuilding.value = selectedBuilding.value === buildingId ? undefined : buildingId
}

const selectedGroup = ref(props.initialSelectedGroup)
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
      <p class="text-2xl mb-2">Für welche Abteilungen interessierst du dich?</p>
      <div class="flex flex-row flex-wrap justify-center gap-2 mb-4">
        <button v-for="department in departments" :key="department.id"
          @click="() => selectDepartment(department.id)"
          class="button text-white"
          :style="{ 'background-color': (selectedDepartments === undefined || selectedDepartments.indexOf(department.id) >= 0 ? department.color : undefined) }">{{ department.longName }}</button>
      </div>
      <p class="text-2xl mb-2">Für welches Gebäude interessierst du dich?</p>
      <div class="flex flex-row flex-wrap justify-center gap-2 mb-4">
        <button v-for="building in buildings" :key="building.id"
          @click="() => selectBuilding(building.id)"
          class="button text-white"
          :class="{ 'button-htlvb-selected': selectedBuilding === building.id }">{{ building.name }}</button>
      </div>
      <p class="text-2xl mb-2">Für welche Projekte interessierst du dich?</p>
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