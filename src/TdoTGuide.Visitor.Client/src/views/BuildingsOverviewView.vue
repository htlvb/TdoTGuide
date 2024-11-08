<script setup lang="ts">
import { computed, ref } from 'vue'
import type { Dto } from '@/Types'
import uiFetch from '@/UIFetch'
import { useRoute } from 'vue-router'
import BuildingOverviewView from './BuildingOverviewView.vue';

const route = useRoute()

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

const buildings = computed(() => {
  if (projectList.value === undefined) return []
  if (route.params.buildingId) {
    return projectList.value.buildings.filter(v => v.id === route.params.buildingId)
  }
  return projectList.value.buildings
})

const zoom = (typeof route.query.zoom === 'string' && parseFloat(route.query.zoom)) || 1
</script>

<template>
  <div v-for="building in buildings" :key="building.id" class="h-full">
    <BuildingOverviewView v-if="projectList !== undefined"
      :building-name="building.name"
      :projects="projectList.projects.filter(v => v.building === building.id)"
      :zoom="zoom"/>
  </div>
</template>