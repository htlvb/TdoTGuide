<script setup lang="ts">
import { ref } from 'vue'
import type { Dto } from '@/Types'
import ExpandableName from './ExpandableName.vue'
import ImageMedia from './ImageMedia.vue'
import MarkdownIt from 'markdown-it'

const props = defineProps<{
  project: Dto.Project
  departments: Dto.Department[]
  buildings: Dto.Building[]
}>()

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

const showMedia = ref(false)
</script>

<template>
  <div class="border rounded p-4 flex flex-col gap-2">
    <div class="flex flex-col gap-2 grow">
      <h3 class="text-xl">{{ project.title }}</h3>
      <p class="description" v-html="projectDescription"></p>
      <div v-if="projectDepartments.length > 0" class="flex flex-wrap gap-2">
        <ExpandableName v-for="department in projectDepartments" :key="department.id" :short-name="department.name" :long-name="department.longName" class="button !text-white" :style="{ 'background-color': department.color}" />
      </div>
      <p><span class="font-bold">Wo:</span> {{ project.location }} ({{ buildings.find(v => v.id === project.building)?.name ?? "unbekanntes Gebäude" }})</p>
    </div>
    <div v-if="showMedia">
      <div class="flex flex-col md:flex-row md:flex-wrap items-center gap-2">
        <template v-for="media in project.media" :key="media.url">
          <ImageMedia v-if="media.type === 'Image'" :url="media.url" />
          <video v-else-if="media.type === 'Video'" :src="media.url" width="300" controls></video>
        </template>
      </div>
    </div>
    <a v-if="project.media.length > 0" @click="showMedia = !showMedia" class="bg-gray-500/25 cursor-pointer flex justify-center p-1">
      <font-awesome-icon :icon="['fas', (showMedia ? 'angles-up' : 'angles-down')]" />
    </a>
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
    @apply text-blue-300 border-blue-300 cursor-pointer hover:border-b;
}
</style>
