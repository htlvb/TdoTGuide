import { groupBy, sortBy, toPairs, uniqBy } from "lodash-es"
import type { Dto } from "./Types"

const getSortOrder = (floor: string) => {
  if (floor === '') return Number.MIN_SAFE_INTEGER
  
  const ugMatch = floor.match(/^(:?(\d+)\.\s*)?(Untergeschoss|UG)$/)
  if (ugMatch !== null) return ugMatch[1] || -1
  
  if (/^(EG|Erdgeschoss)$/.test(floor)) return 0

  const ogMatch = floor.match(/^(:?(\d+)\.\s*)?Stock$/)
  if (ogMatch !== null) return ogMatch[1] || 1

  console.warn(`Failed to determine sort order of floor "${floor}", returning 0`)
  return 0
}

export const groupProjectsByFloor = (projects: Dto.Project[]) => {
    return sortBy(toPairs(groupBy(projects, v => v.floor || '')), ([floor, _projects]) => getSortOrder(floor))
}

export const applyPrintConfig = (projects: Dto.Project[]) => {
  return uniqBy(projects.filter(v => v.showInPrintOverview), v => v.printOverviewGroupName || v.id)
}