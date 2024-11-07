<script setup lang="ts">
import type { Dto } from '@/Types'
import { groupBy, sortBy, toPairs } from 'lodash-es'

defineProps<{
  projects: Dto.Project[]
}>()

const getSortOrder = (floor: string) => {
  if (floor === '') return Number.MIN_SAFE_INTEGER
  
  const ugMatch = floor.match(/^(:?(\d+)\.\s*)?(Untergeschoss|UG)$/)
  if (ugMatch !== null) return ugMatch[1] || -1
  
  if (/^(EG|Erdgeschoss)$/.test(floor)) return 0

  const ogMatch = floor.match(/^(:?(\d+)\.\s*)?Stock$/)
  if (ogMatch !== null) return ogMatch[1] || 1

  console.warn(`Failed to determine sort order of floor "${floor}", returning 0`)
  return 0
}
</script>
<template>
  <div>
    <div v-for="[floor, projects] in sortBy(toPairs(groupBy(projects, v => v.floor || '')), ([floor, _projects]) => getSortOrder(floor))" :key="floor"
      class="flex flex-col gap-4">
      <div v-if="floor !== ''" class="text-center text-3xl small-caps">{{ floor }}</div>
      <ol contenteditable="true" class="list-decimal list-outside ml-8">
        <li v-for="project in projects" :key="project.id" class="text-lg">{{ project.title }}</li>
      </ol>
    </div>
  </div>
</template>