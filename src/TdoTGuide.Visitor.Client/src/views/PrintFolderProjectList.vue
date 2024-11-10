<script setup lang="ts">
import type { Dto } from '@/Types'
import { applyPrintConfig, groupProjectsByFloor } from '@/Utils'

defineProps<{
  projects: Dto.Project[]
}>()
</script>
<template>
  <div>
    <div v-for="[floor, projects] in groupProjectsByFloor(projects).map(([floor, projects]): [string, Dto.Project[]] => [floor, applyPrintConfig(projects)])" :key="floor"
      class="flex flex-col gap-4">
      <div v-if="floor !== ''" class="text-center text-3xl small-caps">{{ floor }}</div>
      <ol contenteditable="true" class="list-decimal list-outside ml-8">
        <li v-for="project in projects" :key="project.id" class="text-lg">{{ project.printOverviewGroupName || project.title }}</li>
      </ol>
    </div>
  </div>
</template>