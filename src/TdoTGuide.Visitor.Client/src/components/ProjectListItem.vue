<script setup lang="ts">
import type { Dto } from '@/Types'
import ExpandableName from './ExpandableName.vue'
import { useTourStore } from '@/stores/tour'
import MarkdownIt from 'markdown-it'
import _, { type Dictionary } from 'lodash'
import { computed } from 'vue'

const props = defineProps<{
  project: Dto.Project,
  departments: Dto.Department[]
}>()

const tourStore = useTourStore()

const md = new MarkdownIt({ linkify: true })
const defaultRender = md.renderer.rules.link_open || ((tokens, idx, options, env, self) => {
  return self.renderToken(tokens, idx, options)
});
md.renderer.rules.link_open = (tokens, idx, options, env, self) => {
  tokens[idx].attrSet('target', '_blank')
  return defaultRender(tokens, idx, options, env, self)
};
const projectDescription = md.render(props.project.description)

const projectDepartments = props.project.departments
  .map(departmentId => props.departments.find(v => v.id === departmentId))
  .filter((v): v is NonNullable<typeof v> => v !== undefined)
</script>

<template>
  <div class="border rounded p-4 flex flex-col gap-2">
    <div class="flex flex-row gap-4">
      <div class="flex flex-col gap-2 grow">
        <h3 class="text-xl">{{ project.title }}</h3>
        <p class="description" v-html="projectDescription"></p>
        <div v-if="projectDepartments.length > 0" class="flex flex-wrap gap-2">
          <ExpandableName v-for="department in projectDepartments" :key="department.id" :short-name="department.name" :long-name="department.longName" class="button !text-white" :style="{ 'background-color': department.color}" />
        </div>
        <p><span class="font-bold">Wo:</span> {{ project.location }}</p>
      </div>
      <div class="flex items-center">
        <a :class="['button', 'text-nowrap', { 'button-htlvb-selected': tourStore.projectIds.indexOf(project.id) >= 0 }]" @click="tourStore.toggleAdd(project.id)">Das interessiert mich</a>
      </div>
    </div>
    <div class="flex flex-row flex-wrap items-center gap-2">
      <template v-for="media in project.media" :key="media.url">
        <img v-if="media.type === 'Image'" :src="media.url" width="200" />
        <video v-else-if="media.type === 'Video'" :src="media.url" width="300" controls></video>
      </template>
    </div>
  </div>
</template>

<style scoped>
.description {
    @apply leading-6;
}
.description :deep(h1) {
    @apply text-lg;
}
.description :deep(ol) {
    @apply list-inside list-decimal;
}
.description :deep(ul) {
    @apply list-inside list-disc;
}
.description :deep(a) {
    @apply text-blue-700 dark:text-blue-300 border-blue-700 dark:border-blue-300 cursor-pointer hover:border-b;
}
</style>
