<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Dto } from '@/Types'
import ProjectListItem from './ProjectListItem.vue'
import _ from 'lodash-es'

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
    }
    else {
      selectedDepartments.value.push(departmentId)
    }
  }
}

const filteredProjects = computed(() => {
  if (selectedDepartments.value === undefined) return props.projects

  return props.projects.filter(project =>
    project.departments.length === 0
    || _.intersection(project.departments, selectedDepartments.value).length > 0
  )
})
</script>

<template>
  <div class="p-4">
    <div class="flex flex-col items-center gap-2">
      <span>Für welche Abteilungen interessierst du dich?</span>
      <div class="flex flex-row flex-wrap justify-center gap-2">
        <a v-for="department in departments" :key="department.id"
          @click="() => selectDepartment(department.id)"
          class="button !text-white"
          :style="{ 'background-color': (selectedDepartments === undefined || selectedDepartments.indexOf(department.id) >= 0 ? department.color : undefined) }">{{ department.longName }}</a>
      </div>
    </div>
    <div class="flex flex-col gap-4 mt-4">
      <span v-if="filteredProjects.length === 0" class="self-center">
        😥 Keine Angebote gefunden
      </span>
      <ProjectListItem v-for="project in filteredProjects" :key="JSON.stringify(project)" :project="project"
        :departments="departments" class="border rounded p-4 flex flex-col gap-2" />
    </div>
  </div>
</template>