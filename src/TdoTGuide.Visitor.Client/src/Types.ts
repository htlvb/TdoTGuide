export module Dto {
  export type ProjectList = {
    projects: Project[]
    departments: Department[]
  }

  export type Project = {
    id: string
    title: string
    description: string
    group: string | null
    departments: string[]
    location: string
    media: ProjectMedia[]
  }

  export type Department = {
    id: string
    name: string
    longName: string
    color: string
  }

  export type ProjectMedia = {
    type: ProjectMediaType
    url: string
  }

  export type ProjectMediaType = 'Image' | 'Video'
}
