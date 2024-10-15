export module Dto {
  export type ProjectList = {
    projects: Project[]
    departments: Department[]
    buildings: Building[]
  }

  export type Project = {
    id: string
    title: string
    description: string
    groups: string[]
    departments: string[]
    building: string
    location: string
    media: ProjectMedia[]
  }

  export type Department = {
    id: string
    name: string
    longName: string
    color: string
  }

  export type Building = {
    id: string
    name: string
  }

  export type ProjectMedia = {
    type: ProjectMediaType
    url: string
  }

  export type ProjectMediaType = 'Image' | 'Video'
}
