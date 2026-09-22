import { useEffect, useState } from 'react';
import { dashboardService, DashboardSummaryDto } from '../../services/dashboardService';
import { Activity, Car, AlertTriangle, Users } from 'lucide-react';
import './Dashboard.css';

const Dashboard = () => {
  const [summary, setSummary] = useState<DashboardSummaryDto | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadData = async () => {
      try {
        const data = await dashboardService.getSummary();
        // Since DashboardController returns raw object (or ApiResponse), handle both
        setSummary(data.data || data);
      } catch (err) {
        console.error('Failed to load dashboard data', err);
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  return (
    <div className="dashboard-container fade-in">
      <h1 className="page-title">Dashboard Overview</h1>
      
      {loading ? (
        <div style={{textAlign: 'center', padding: '3rem', color: 'var(--text-muted)'}}>
          Loading dashboard data...
        </div>
      ) : (
        <>
          <div className="stats-grid">
            <div className="stat-card glass-panel">
              <div className="stat-icon bg-primary">
                <Car size={24} color="white" />
              </div>
              <div className="stat-details">
                <h3>Total Vehicles</h3>
                <p className="stat-value">{summary?.totalVehicles || 0}</p>
              </div>
            </div>
            
            <div className="stat-card glass-panel">
              <div className="stat-icon bg-success">
                <Users size={24} color="white" />
              </div>
              <div className="stat-details">
                <h3>Total Drivers</h3>
                <p className="stat-value">{summary?.totalDrivers || 0}</p>
              </div>
            </div>
            
            <div className="stat-card glass-panel">
              <div className="stat-icon bg-warning">
                <Activity size={24} color="white" />
              </div>
              <div className="stat-details">
                <h3>Maintenance</h3>
                <p className="stat-value">{summary?.vehiclesUnderMaintenance || 0}</p>
              </div>
            </div>
            
            <div className="stat-card glass-panel">
              <div className="stat-icon bg-danger">
                <AlertTriangle size={24} color="white" />
              </div>
              <div className="stat-details">
                <h3>Open Incidents</h3>
                <p className="stat-value">{summary?.openIncidentsCount || 0}</p>
              </div>
            </div>
          </div>
          
          <div className="dashboard-content">
            <div className="recent-activity glass-panel">
              <h2>Recent Alerts</h2>
              <div className="activity-list">
                {/* We can load alerts here later */}
                <div className="activity-item">
                  <div className="activity-dot bg-warning"></div>
                  <p>System initialized successfully</p>
                  <span className="time">Just now</span>
                </div>
              </div>
            </div>
            
            <div className="chart-placeholder glass-panel">
              <h2>Fleet Utilization</h2>
              <div className="placeholder-box">
                 <div className="bar-chart">
                   <div className="bar" style={{height: '60%'}}></div>
                   <div className="bar" style={{height: '80%'}}></div>
                   <div className="bar" style={{height: '40%'}}></div>
                   <div className="bar" style={{height: '90%'}}></div>
                   <div className="bar" style={{height: '70%'}}></div>
                 </div>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default Dashboard;
