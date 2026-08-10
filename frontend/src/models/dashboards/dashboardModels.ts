export type VisualizationType = 'Table' | 'Number' | 'BarChart'

export interface AnalyticsDashboardWidget {
  id: string
  title: string
  savedQueryId: string
  visualizationType: VisualizationType
  displayOrder: number
}

export interface AnalyticsDashboard {
  id: string
  name: string
  isDynamic: boolean
  widgets: AnalyticsDashboardWidget[]
  createdByUserId?: string
  createdAtUtc?: string
  updatedAtUtc?: string
}

export interface CreateAnalyticsDashboardWidgetRequest {
  title: string
  savedQueryId: string
  visualizationType: VisualizationType
  displayOrder: number
}

export interface CreateAnalyticsDashboardRequest {
  name: string
  isDynamic: boolean
  widgets: CreateAnalyticsDashboardWidgetRequest[]
}
