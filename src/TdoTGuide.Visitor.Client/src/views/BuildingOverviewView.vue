<script setup lang="ts">
import { computed } from 'vue'
import type { Dto } from '@/Types'
import { groupProjectsByFloor } from '@/Utils'

const props = withDefaults(defineProps<{
  buildingName: string
  projects: Dto.Project[]
  zoom: number | undefined
}>(), { zoom: 1 })
const groupedProjects = computed(() => groupProjectsByFloor(props.projects))
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex-grow grid grid-flow-col auto-cols-fr items-center">
      <div v-for="[floor, projects] in groupedProjects" :key="floor" class="p-4">
        <div class="text-xl text-center" :style="{ zoom: zoom }">{{ floor }}</div>
        <ol class="list-decimal">
          <li v-for="project in projects" :key="project.id" class="ml-8" :style="{ zoom: zoom }">{{ project.title }}</li>
        </ol>
      </div>
    </div>
    <div class="p-4 text-3xl text-center" :style="{ zoom: zoom }">{{ buildingName }}</div>
  </div>
</template>