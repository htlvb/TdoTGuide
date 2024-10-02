<script setup lang="ts">
import { ref } from 'vue'

defineProps<{
    url: string
}>()

const isBig = ref(false)
</script>

<template>
    <img :src="url" :class="{ 'w-[200px] cursor-zoom-in': !isBig, 'w-full cursor-zoom-out': isBig }" @click="isBig = !isBig" />
</template>