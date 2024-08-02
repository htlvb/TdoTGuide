<script setup lang="ts">
import { computed, ref } from 'vue';

const props = defineProps<{
    shortName: string
    longName: string
}>()
const isShortName = ref(true)
const name = computed(() => isShortName.value ? props.shortName : props.longName)
</script>

<template>
    <div @click="isShortName = !isShortName">{{ name }}</div>
</template>