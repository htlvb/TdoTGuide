import { defineStore } from 'pinia'

export const useTourStore = defineStore('tour', {
  state: () => {
    try {
      const tour = JSON.parse(localStorage.getItem('tour') || "{}")
      if (tour.version === 1 && Array.isArray(tour.data)) {
        return { projectIds: tour.data.filter((v: any) => typeof v === 'string') }
      }
      else {
        return { projectIds: [] as string[] }
      }
    }
    catch {
      return { projectIds: [] as string[] }
    }
  },
  actions: {
    toggleAdd(projectId: string) {
      const index = this.projectIds.indexOf(projectId)
      if (index >= 0) {
        this.projectIds.splice(index, 1)
      }
      else {
        this.projectIds.push(projectId)
      }
      localStorage.setItem('tour', JSON.stringify({ version: 1, data: this.projectIds }))
    },
  },
})
