<script setup lang="ts">
import { computed } from 'vue'
import type { Dto } from '@/Types'
import _ from 'lodash';

const props = defineProps<{
  departments: Dto.Department[]
  groups: string[]
  buildings: Dto.Building[]
}>()

const emit = defineEmits<{
  selectDepartment: [Dto.Department['id']]
  selectGroup: [string]
  selectBuilding: [Dto.Building['id']]
}>()

const filters = computed(() => {
  let id = 1
  const sortedFilters = _([
      ...props.departments.map(v => ({ id: id++, title: v.longName, onClick: (() => emit('selectDepartment', v.id)) })),
      ...props.groups.map(v => ({ id: id++, title: v, onClick: (() => emit('selectGroup', v)) })),
      ...props.buildings.map(v => ({ id: id++, title: v.name, onClick: (() => emit('selectBuilding', v.id)) }))
    ])
    .sortBy(v => v.title.length)
    .value()
  const [top, bottom] = [[] as typeof sortedFilters, [] as typeof sortedFilters]
  for (let i = 0; i < sortedFilters.length; i++) {
    if (i % 2 == 0) top.push(sortedFilters[i])
    else bottom.push(sortedFilters[i])
  }
  return [...top, ...bottom.reverse()]
})
</script>

<template>
  <div class="flex flex-col gap-4 items-center">
    <header class="text-center small-caps mt-16">
      <p class="text-5xl leading-normal">Tage der offenen Tür</p>
      <p class="text-3xl leading-normal">Fr. 22.11.2024 13:00 - 17:00</p>
      <p class="text-3xl leading-normal">Sa. 23.11.2024 09:00 - 13:00</p>
    </header>
    <div class="border border-white border-2 p-4 text-3xl leading-normal text-center small-caps">
      <p>Gestalte deinen eigenen Rundgang</p>
      <p>Suche dir aus, was dir gefällt</p>
    </div>
    <div class="my-8 flex flex-col items-center">
      <a v-for="filter in filters" :key="filter.id" class="filter" @click="filter.onClick">
        {{ filter.title }}
      </a>
    </div>
    <div>
      <img src="@/assets/logo.svg" width="250">
    </div>
  </div>
</template>

<style lang="css" scoped>
.filter {
  @apply text-2xl cursor-pointer text-center my-4 [font-variant:small-caps];
}
</style>
