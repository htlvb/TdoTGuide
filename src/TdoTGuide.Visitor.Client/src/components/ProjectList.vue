<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Dto } from '@/Types'
import ProjectListItem from './ProjectListItem.vue'
import _ from 'lodash-es'
import { useTourStore } from '@/stores/tour';

const props = defineProps<{
  projects: Dto.Project[]
  departments: Dto.Department[]
}>()

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

const tourStore = useTourStore()

const showMyTour = ref(tourStore.projectIds.length > 0)

const filteredProjects = computed(() => {
  let result = props.projects

  if (showMyTour.value) {
    result = result.filter(project => tourStore.projectIds.indexOf(project.id) >= 0)
  }

  if (selectedDepartments.value === undefined) return result

  return result.filter(project =>
    project.departments.length === 0
    || _.intersection(project.departments, selectedDepartments.value).length > 0
  )
})
</script>

<template>
  <div>
    <div class="flex flex-col items-center gap-2">
      <span>Für welche Abteilungen interessierst du dich?</span>
      <div class="flex flex-row flex-wrap justify-center gap-2">
        <button v-for="department in departments" :key="department.id"
          @click="() => selectDepartment(department.id)"
          class="button text-white"
          :style="{ 'background-color': (selectedDepartments === undefined || selectedDepartments.indexOf(department.id) >= 0 ? department.color : undefined) }">{{ department.longName }}</button>
      </div>
      <button :disabled="!showMyTour && tourStore.projectIds.length === 0" :class="['button', 'mt-4', 'self-start', { 'button-htlvb-selected': showMyTour }]" @click="showMyTour = !showMyTour">Nur meine Projekte anzeigen</button>
    </div>
    <div class="flex flex-col gap-4 mt-4">
      <span v-if="filteredProjects.length === 0" class="self-center">
        😥 Keine Angebote gefunden
      </span>
      <ProjectListItem v-for="project in filteredProjects" :key="JSON.stringify(project)" :project="project"
        :departments="departments" />
    </div>
  </div>
</template>