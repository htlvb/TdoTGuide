import type { Dto } from '@/Types'
import { defineStore } from 'pinia'

export const useTourStore = defineStore('tour', {
  state: () => {
    return { projects: [] as Dto.Project[] }
  },
  actions: {
    toggleAdd(project: Dto.Project) {
      const index = this.projects.indexOf(project)
      if (index >= 0) {
        this.projects.splice(index, 1)
      }
      else {
        this.projects.push(project)
      }
    },
  },
})
