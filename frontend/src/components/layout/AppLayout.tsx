import type { ReactNode } from 'react'

export type AppRoutePath = '/ask-data' | '/saved-queries' | '/dashboards' | '/visual-query' | `/dashboards/${string}`

interface NavigationItem {
  path: AppRoutePath
  label: string
}

interface AppLayoutProps {
  currentPath: string
  onNavigate: (path: AppRoutePath) => void
  children: ReactNode
}

const navigationItems: NavigationItem[] = [
  { path: '/ask-data', label: 'Ask Data' },
  { path: '/saved-queries', label: 'Saved Queries' },
  { path: '/dashboards', label: 'Dashboards' },
  { path: '/visual-query', label: 'Visual Query' },
]

export default function AppLayout({ currentPath, onNavigate, children }: AppLayoutProps): JSX.Element {
  return (
    <div className="app-shell">
      <aside className="app-sidebar">
        <div className="app-brand">
          <p className="app-brand__eyebrow">CBS BI Platform</p>
          <h1 className="app-brand__title">Analytics workspace</h1>
          <p className="app-brand__subtitle">Ask data in natural language and review the generated SQL and results.</p>
        </div>

        <nav className="app-nav" aria-label="Primary">
          {navigationItems.map((item) => {
            const isActive = currentPath === item.path || (item.path === '/dashboards' && currentPath.startsWith('/dashboards/'))

            return (
              <a
                key={item.path}
                href={item.path}
                aria-current={isActive ? 'page' : undefined}
                className={isActive ? 'app-nav__link app-nav__link--active' : 'app-nav__link'}
                onClick={(event) => {
                  event.preventDefault()
                  onNavigate(item.path)
                }}
              >
                {item.label}
              </a>
            )
          })}
        </nav>
      </aside>

      <main className="app-content">
        <div className="app-content__inner">
          {children}
        </div>
      </main>
    </div>
  )
}
