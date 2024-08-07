export module Dto {
  export type ProjectList = {
    projects: Project[]
    departments: Department[]
  }

  export type Project = {
    id: string
    title: string
    description: string
    group: string
    departments: string[]
    location: string
    timeSelection: TimeSelection
    media: ProjectMedia[]
  }

  export type Department = {
    id: string
    name: string
    longName: string
    color: string
  }

  export type TimeSelection = {
    type: TimeSelectionType
    regularIntervalMinutes: number
    individualTimes: Date[]
  }

  export type TimeSelectionType = 'Continuous' | 'Regular' | 'Individual'

  export type ProjectMedia = {
    type: ProjectMediaType
    url: string
  }

  export type ProjectMediaType = 'Image' | 'Video'
}
