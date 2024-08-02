<script setup lang="ts">
import type { Dto } from '@/Types'
import ExpandableName from './ExpandableName.vue'

const props = defineProps<{
  project: Dto.Project,
  departments: Dto.Department[]
}>()

const projectDepartments = props.project.departments
  .map(departmentId => props.departments.find(v => v.id === departmentId))
  .filter((v): v is NonNullable<typeof v> => v !== undefined)

const getRegularTimeSelectionText = (intervalMinutes: number) => {
  if (intervalMinutes === 1) return 'jede Minute'
  else if (intervalMinutes < 60) return `alle ${intervalMinutes} Minuten`
  else if (intervalMinutes === 60) return `stündlich`
  else if (intervalMinutes % 60 === 0) return `alle ${intervalMinutes / 60} Stunden`
  else `alle ${intervalMinutes} Minuten`
}

</script>

<template>
  <div>
    <h3>{{ project.title }}</h3>
    <p>{{ project.description }}</p>
    <div v-if="projectDepartments.length > 0" class="flex flex-wrap gap-2">
      <ExpandableName v-for="department in projectDepartments" :key="department.id" :short-name="department.name" :long-name="department.longName" class="button !text-white" :style="{ 'background-color': department.color}" />
    </div>
    <p>
      <span class="font-bold">Wann: </span>
      <span v-if="project.timeSelection.type === 'Continuous'">laufend</span>
      <span v-else-if="project.timeSelection.type === 'Regular'">{{ getRegularTimeSelectionText(project.timeSelection.regularIntervalMinutes) }}</span>
      <ul v-else-if="project.timeSelection.type === 'Individual'" class="list-disc list-inside">
        <li v-for="time in project.timeSelection.individualTimes" :key="time.toISOString()">{{ time.toLocaleString() }}</li>
      </ul>
    </p>
    <p><span class="font-bold">Wo:</span> {{ project.location }}</p>
  </div>
</template>
