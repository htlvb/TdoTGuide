export namespace Dto {
  export type ProjectList = {
    projects: Project[]
    projectTags: ProjectTag[][]
    buildings: Building[]
  }

  export type Project = {
    id: string
    title: string
    description: string
    tags: ProjectTag[]
    building: string
    floor: string | null
    location: string
    media: ProjectMedia[]
  }

  export type ProjectTag = {
    shortName: string | null
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
