<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'

const directions = ['left', 'right'] as const

const departments = [
  { name: 'Wirtschaftsingenieure', dates: ['Termin 1: Freitag, 20.11., 14:45 Uhr', 'Termin 2: Samstag, 21.11., 9:45 Uhr'] },
  { name: 'Gebäudetechnik', dates: ['Termin 1: Freitag, 20.11., 15:45 Uhr', 'Termin 2: Samstag, 21.11., 10:45 Uhr'] },
  { name: 'Mechatronik/Maschinenbau', dates: ['Termin 1: Freitag, 20.11., 16:30 Uhr', 'Termin 2: Samstag, 21.11., 11:30 Uhr'] },
]

// Scale the content to the largest font size that still fits into its box
const contentBoxes = ref<HTMLElement[]>([])
const contents = ref<HTMLElement[]>([])
const headlines = ref<HTMLElement[]>([])
const headlineScale = 1.4
const fitContent = () => {
  contentBoxes.value.forEach((box, i) => {
    const content = contents.value[i]
    const headline = headlines.value[i]
    if (content === undefined || headline === undefined) return
    const setFontSize = (fontSize: number) => {
      content.style.fontSize = `${fontSize}px`
      headline.style.fontSize = `${fontSize * headlineScale}px`
    }
    const fits = (fontSize: number) => {
      setFontSize(fontSize)
      return content.offsetWidth <= box.clientWidth && content.offsetHeight <= box.clientHeight
        && (headline.parentElement === null || headline.parentElement.scrollWidth <= headline.parentElement.clientWidth)
    }
    let min = 1, max = 1000
    while (max - min > 0.5) {
      const mid = (min + max) / 2
      if (fits(mid)) min = mid
      else max = mid
    }
    setFontSize(min)
  })
}
// Keep the text of both pages in sync and refit after every edit
const editables = ref<HTMLElement[]>([])
const onInput = (e: Event) => {
  const source = e.currentTarget as HTMLElement
  editables.value.filter(v => v !== source).forEach(v => v.innerHTML = source.innerHTML)
  fitContent()
}

onMounted(async () => {
  fitContent()
  await document.fonts.ready
  fitContent()
  window.addEventListener('resize', fitContent)
})
onUnmounted(() => window.removeEventListener('resize', fitContent))
</script>

<template>
  <section v-for="direction in directions" :key="direction" class="page">
    <div class="h-full flex flex-col gap-[12mm] p-[20mm]">
      <div class="flex items-center justify-center gap-[15mm]">
        <img src="@/assets/logo.svg" class="shrink-0 w-[110mm]">
        <h1 ref="headlines" class="shrink-0 w-max border-white border-[0.06em] px-[0.4em] py-[0.15em] text-center leading-tight small-caps">
          Abteilungspräsentationen
        </h1>
      </div>
      <div class="grow min-h-0 w-full flex items-center gap-[10mm]" :class="{ 'flex-row-reverse': direction === 'right' }">
        <svg viewBox="0 0 82 60" class="shrink-0 w-[107mm]" :class="{ '-scale-x-100': direction === 'right' }"
          fill="none" stroke="currentColor" stroke-width="7" stroke-linecap="round" stroke-linejoin="round">
          <path d="M76 30 H6 M28 8 L6 30 L28 52" />
        </svg>
        <div ref="contentBoxes" class="grow self-stretch min-w-0 flex items-center justify-center">
          <div ref="contents" class="w-max leading-snug whitespace-nowrap">
            <div ref="editables" contenteditable class="outline-none" @input="onInput">
              <div v-for="(department, index) in departments" :key="department.name" :class="{ 'mt-[0.6em]': index > 0 }">
                <h2 class="uppercase font-bold tracking-wide">{{ department.name }}</h2>
                <p v-for="date in department.dates" :key="date" class="ml-[1em]">{{ date }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style lang="css" scoped>
/* A2 landscape (594mm x 420mm) + 3mm bleed on each side */
@page {
  size: 600mm 426mm;
  margin: 0;
}
.page {
  width: 600mm;
  height: 426mm;
  padding: 3mm;
  overflow: hidden;
  break-after: page;
}
.page:last-child {
  break-after: auto;
}
</style>
