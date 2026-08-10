import type {
  VisualQueryDomain,
  VisualQueryMetric,
  VisualQuerySortDirection,
} from '../models/visual-query/visualQueryModels'

export interface VisualQueryDomainOption {
  domain: VisualQueryDomain
  metrics: readonly VisualQueryMetric[]
}

export type VisualQueryYearOptionId = 'latest' | '2025' | '2024'

export interface VisualQueryYearOption {
  id: VisualQueryYearOptionId
  label: string
  year: number | null
}

export type VisualQuerySortOptionId = 'highest' | 'lowest'

export interface VisualQuerySortOption {
  id: VisualQuerySortOptionId
  label: string
  sortDirection: VisualQuerySortDirection
}

export const VISUAL_QUERY_DOMAIN_OPTIONS: readonly VisualQueryDomainOption[] = [
  {
    domain: 'Population',
    metrics: ['Population'],
  },
  {
    domain: 'Employment',
    metrics: ['UnemploymentRatePct', 'EmployedPersons', 'AverageMonthlySalaryNis'],
  },
  {
    domain: 'Housing',
    metrics: ['AverageApartmentPriceNis', 'AverageMonthlyRentNis'],
  },
  {
    domain: 'Education',
    metrics: ['TotalStudents', 'MatriculationEligibilityPct', 'TeachersCount'],
  },
] as const

export const VISUAL_QUERY_YEAR_OPTIONS: readonly VisualQueryYearOption[] = [
  { id: 'latest', label: 'Latest', year: null },
  { id: '2025', label: '2025', year: 2025 },
  { id: '2024', label: '2024', year: 2024 },
] as const

export const VISUAL_QUERY_SORT_OPTIONS: readonly VisualQuerySortOption[] = [
  { id: 'highest', label: 'Highest', sortDirection: 'Descending' },
  { id: 'lowest', label: 'Lowest', sortDirection: 'Ascending' },
] as const

export const DEFAULT_VISUAL_QUERY_DOMAIN: VisualQueryDomain = VISUAL_QUERY_DOMAIN_OPTIONS[0].domain

export const DEFAULT_VISUAL_QUERY_METRIC: VisualQueryMetric = VISUAL_QUERY_DOMAIN_OPTIONS[0].metrics[0]

export const DEFAULT_VISUAL_QUERY_LIMIT = 5

export const MIN_VISUAL_QUERY_LIMIT = 1

export const MAX_VISUAL_QUERY_LIMIT = 100

export const DEFAULT_VISUAL_QUERY_YEAR_OPTION_ID: VisualQueryYearOptionId = 'latest'

export const DEFAULT_VISUAL_QUERY_SORT_OPTION_ID: VisualQuerySortOptionId = 'highest'

export function getMetricsForDomain(domain: VisualQueryDomain): readonly VisualQueryMetric[] {
  const domainOption = VISUAL_QUERY_DOMAIN_OPTIONS.find((option) => option.domain === domain)

  return domainOption ? domainOption.metrics : []
}

export function getYearByOptionId(optionId: VisualQueryYearOptionId): number | null {
  return VISUAL_QUERY_YEAR_OPTIONS.find((option) => option.id === optionId)?.year ?? null
}

export function getSortDirectionByOptionId(optionId: VisualQuerySortOptionId): VisualQuerySortDirection {
  return VISUAL_QUERY_SORT_OPTIONS.find((option) => option.id === optionId)?.sortDirection ?? 'Descending'
}
