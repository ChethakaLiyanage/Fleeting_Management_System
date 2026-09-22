import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { dashboardService, DashboardSummaryDto } from '../../services/dashboardService';
import { Activity, Car, AlertTriangle, Users, Droplet, Wrench, FileCheck, Map } from 'lucide-react';
import './Dashboard.css';

const Dashboard = () => {
  const [summary, setSummary] = useState<DashboardSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadData = async () => {
      try {
        const data = await dashboardService.getSummary();
        setSummary((data as any).data || data);
      } catch (err) {
        console.error('Failed to load dashboard data', err);
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  const stats = [
    {
      label: 'Total Vehicles',
      value: summary?.totalVehicles ?? 0,
      icon: <Car size={20} />,
      color: 'var(--primary)',
      bg: 'var(--primary-light)',
      sub: `${summary?.availableVehicles ?? 0} available`,
    },
    {
      label: 'Total Drivers',
      value: summary?.totalDrivers ?? 0,
      icon: <Users size={20} />,
      color: 'var(--success)',
      bg: 'var(--success-light)',
      sub: `${summary?.availableDrivers ?? 0} available`,
    },
    {
      label: 'Active Trips',
      value: summary?.activeTrips ?? 0,
      icon: <Activity size={20} />,
      color: '#1d4ed8',
      bg: 'var(--info-light)',
      sub: 'In progress',
    },
    {
      label: 'In Maintenance',
      value: summary?.vehiclesUnderMaintenance ?? 0,
      icon: <Wrench size={20} />,
      color: '#b45309',
      bg: 'var(--accent-light)',
      sub: 'Vehicles off-road',
    },
    {
      label: 'Open Incidents',
      value: summary?.openIncidentsCount ?? 0,
      icon: <AlertTriangle size={20} />,
      color: 'var(--danger)',
      bg: '#fee2e2',
      sub: 'Requiring attention',
    },
  ];

  return (
    <div className="fade-in">
      {/* Page Header */}
      <div className="page-header">
        <div className="page-header-left">
          <h1>Dashboard</h1>
          <p>Welcome back! Here's your fleet at a glance.</p>
        </div>
        <div style={{ display: 'flex', gap: '0.75rem' }}>
          <span style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', alignSelf: 'center' }}>
            {new Date().toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}
          </span>
        </div>
      </div>

      {loading ? (
        <div className="state-container"><span className="spinner" style={{ border: '2px solid #e5e7eb', borderTop: '2px solid var(--primary)', width: 28, height: 28 }} /><p>Loading dashboard...</p></div>
      ) : (
        <>
          {/* KPI Stat Cards */}
          <div className="stats-grid">
            {stats.map((stat, i) => (
              <div className="stat-card" key={i} style={{ '--stat-color': stat.color, '--stat-bg': stat.bg } as any}>
                <div className="stat-icon-wrap">{stat.icon}</div>
                <div className="stat-info">
                  <div className="stat-label">{stat.label}</div>
                  <div className="stat-value">{stat.value}</div>
                  <div className="stat-sub">{stat.sub}</div>
                </div>
              </div>
            ))}
          </div>

          {/* Bottom Grid */}
          <div className="dashboard-grid">
            {/* Recent Activity */}
            <div className="activity-card">
              <div className="activity-card-title">Recent Activity</div>
              <div className="activity-list">
                <div className="activity-item">
                  <div className="activity-dot dot-success" />
                  <p>Fleet Management System initialized successfully</p>
                  <span className="activity-time">Just now</span>
                </div>
                <div className="activity-item">
                  <div className="activity-dot dot-primary" />
                  <p>Admin account seeded and ready</p>
                  <span className="activity-time">Today</span>
                </div>
                <div className="activity-item">
                  <div className="activity-dot dot-warning" />
                  <p>{summary?.vehiclesUnderMaintenance ?? 0} vehicle(s) under maintenance</p>
                  <span className="activity-time">Current</span>
                </div>
                {(summary?.openIncidentsCount ?? 0) > 0 && (
                  <div className="activity-item">
                    <div className="activity-dot dot-primary" style={{ background: 'var(--danger)' }} />
                    <p>{summary?.openIncidentsCount} open incident(s) require attention</p>
                    <span className="activity-time">Now</span>
                  </div>
                )}
              </div>
            </div>

            {/* Fleet Utilization Bar Chart */}
            <div className="activity-card">
              <div className="activity-card-title">Fleet Utilization</div>
              {(!summary || (
                (summary.availableVehicles ?? 0) === 0 && 
                (summary.activeTrips ?? 0) === 0 && 
                (summary.vehiclesUnderMaintenance ?? 0) === 0 && 
                (summary.openIncidentsCount ?? 0) === 0 && 
                (summary.availableDrivers ?? 0) === 0
              )) ? (
                <div style={{ height: '260px', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--text-muted)', fontSize: '0.875rem' }}>
                  No fleet activity data available yet.
                </div>
              ) : (
                <div className="chart-wrap">
                  {[
                    { label: 'Available', value: summary?.availableVehicles, height: summary?.availableVehicles ? Math.min(100, (summary.availableVehicles / (summary.totalVehicles || 1)) * 100) : 0, accent: false },
                    { label: 'Trips', value: summary?.activeTrips, height: summary?.activeTrips ? Math.min(100, (summary.activeTrips / (summary.totalVehicles || 1)) * 100) : 0, accent: false },
                    { label: 'Maint.', value: summary?.vehiclesUnderMaintenance, height: summary?.vehiclesUnderMaintenance ? Math.min(100, (summary.vehiclesUnderMaintenance / (summary.totalVehicles || 1)) * 100) : 0, accent: true },
                    { label: 'Incidents', value: summary?.openIncidentsCount, height: summary?.openIncidentsCount ? Math.min(100, (summary.openIncidentsCount / (summary.totalVehicles || 1)) * 100) : 0, accent: true },
                    { label: 'Drivers', value: summary?.availableDrivers, height: summary?.availableDrivers ? Math.min(100, (summary.availableDrivers / (summary.totalDrivers || 1)) * 100) : 0, accent: false },
                  ].map((bar, i) => (
                    <div key={i} className="chart-bar-group">
                      <div className="chart-bar-container">
                        <div className="chart-bar-value">{Number(bar.value ?? 0)}</div>
                        <div className={`chart-bar ${bar.accent ? 'accent' : ''}`} style={{ height: `max(4px, ${bar.height}%)` }} />
                      </div>
                      <div className="chart-bar-label">{bar.label}</div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Quick Action Links */}
          <div style={{ marginTop: '1.5rem' }}>
            <h3 style={{ fontSize: '0.9375rem', fontWeight: 600, marginBottom: '1rem', color: 'var(--text-muted)' }}>Quick Actions</h3>
            <div className="quick-actions">
              {[
                { label: 'Add Vehicle', to: '/vehicles/new', icon: <Car size={20} />, color: 'var(--primary)', bg: 'var(--primary-light)' },
                { label: 'Add Driver', to: '/drivers/new', icon: <Users size={20} />, color: 'var(--success)', bg: 'var(--success-light)' },
                { label: 'Log Trip', to: '/trips/new', icon: <Map size={20} />, color: '#1d4ed8', bg: 'var(--info-light)' },
                { label: 'Log Fuel', to: '/fuel/new', icon: <Droplet size={20} />, color: '#1d4ed8', bg: 'var(--info-light)' },
                { label: 'Maintenance', to: '/maintenance/new', icon: <Wrench size={20} />, color: '#b45309', bg: 'var(--accent-light)' },
                { label: 'Inspection', to: '/inspections/new', icon: <FileCheck size={20} />, color: 'var(--success)', bg: 'var(--success-light)' },
                { label: 'Report Incident', to: '/incidents/new', icon: <AlertTriangle size={20} />, color: 'var(--danger)', bg: '#fee2e2' },
              ].map((action, i) => (
                <Link key={i} to={action.to} className="quick-action-card">
                  <div className="quick-action-icon" style={{ background: action.bg, color: action.color }}>{action.icon}</div>
                  <div className="quick-action-label">{action.label}</div>
                </Link>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default Dashboard;
