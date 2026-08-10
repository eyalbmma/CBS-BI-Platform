import { apiRequest } from '../http/apiClient'
import type {
  AnalyticsDashboard,
  CreateAnalyticsDashboardRequest,
} from '../../models/dashboards/dashboardModels'

const DASHBOARDS_PATH = '/api/analytics/dashboards'

export function getDashboards(signal?: AbortSignal): Promise<AnalyticsDashboard[]> {
  return apiRequest<AnalyticsDashboard[]>({
    method: 'GET',
    path: DASHBOARDS_PATH,
    signal,
  })
}

export function getDashboardById(id: string, signal?: AbortSignal): Promise<AnalyticsDashboard> {
  return apiRequest<AnalyticsDashboard>({
    method: 'GET',
    path: `${DASHBOARDS_PATH}/${encodeURIComponent(id)}`,
    signal,
  })
}

export function createDashboard(request: CreateAnalyticsDashboardRequest, signal?: AbortSignal): Promise<AnalyticsDashboard> {
  return apiRequest<AnalyticsDashboard, CreateAnalyticsDashboardRequest>({
    method: 'POST',
    path: DASHBOARDS_PATH,
    body: request,
    signal,
  })
}

export function deleteDashboard(id: string, signal?: AbortSignal): Promise<void> {
  return apiRequest<void>({
    method: 'DELETE',
    path: `${DASHBOARDS_PATH}/${encodeURIComponent(id)}`,
    signal,
  })
}
