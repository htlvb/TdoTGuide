<script setup lang="ts">
import type { Dto } from '@/Types'
import ExpandableName from './ExpandableName.vue'
import { useTourStore } from '@/stores/tour';

const props = defineProps<{
  project: Dto.Project,
  departments: Dto.Department[]
}>()

const tourStore = useTourStore()

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
  <div class="border rounded p-4 flex flex-col gap-2">
    <div class="flex flex-row">
      <div class="flex flex-col gap-2 grow">
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
      <div class="flex items-center">
        <a :class="['button', 'text-nowrap', { 'button-htlvb-selected': tourStore.projects.indexOf(project) >= 0 }]" @click="tourStore.toggleAdd(project)">Das interessiert mich</a>
      </div>
    </div>
    <div class="flex flex-row flex-wrap gap-2">
      <template v-for="media in project.media" :key="media.url">
        <img v-if="media.type === 'Image'" :src="media.url" width="200" />
        <video v-else-if="media.type === 'Video'" :src="media.url" width="300" controls></video>
      </template>
    </div>
  </div>
</template>
