<script setup lang="ts">
import { computed } from 'vue'
import type { Dto } from '@/Types'
import { applyPrintConfig, groupProjectsByFloor } from '@/Utils'

const props = withDefaults(defineProps<{
  buildingName: string
  projects: Dto.Project[]
  zoom: number | undefined
}>(), { zoom: 1 })
const groupedProjects = computed(() => groupProjectsByFloor(props.projects).map(([floor, projects]): [string, Dto.Project[]] => [floor, applyPrintConfig(projects)]))
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="p-4 text-3xl text-center" :style="{ zoom: zoom }">{{ buildingName }}</div>
    <div class="flex-grow grid grid-flow-col auto-cols-fr">
      <div v-for="[floor, projects] in groupedProjects" :key="floor" class="p-4">
        <div class="text-xl text-center" :style="{ zoom: zoom }">{{ floor }}</div>
        <ol class="list-decimal">
          <li v-for="project in projects" :key="project.id" class="ml-8" :style="{ zoom: zoom }">{{ project.printOverviewGroupName || project.title }}</li>
        </ol>
      </div>
    </div>
  </div>
</template>