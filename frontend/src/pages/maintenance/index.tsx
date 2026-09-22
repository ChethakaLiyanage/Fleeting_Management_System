import { useEffect, useState } from 'react';
import { maintenanceService } from '../../services/maintenanceService';
import { MaintenanceRecordDto, PagedResult } from '../../types';
import { Plus, Search, Filter } from 'lucide-react';
import '../vehicles/Vehicles.css';

const Maintenance = () => {
  const [data, setData] = useState<PagedResult<MaintenanceRecordDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchMaintenance();
  }, []);

  const fetchMaintenance = async () => {
    try {
      setLoading(true);
      const res = await maintenanceService.getMaintenanceRecords();
      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load maintenance records');
      }
    } catch (err) {
      setError('Network error: Could not reach the server');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="vehicles-container fade-in">
      <div className="page-header">
        <h1>Maintenance & Servicing</h1>
        <button className="btn btn-primary">
          <Plus size={18} /> Schedule Maintenance
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search maintenance records..." />
        </div>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading records...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchMaintenance}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Vehicle ID</th>
                <th>Date</th>
                <th>Description</th>
                <th>Provider</th>
                <th>Cost</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map(record => (
                  <tr key={record.id}>
                    <td><strong>{record.vehicleId}</strong></td>
                    <td>{new Date(record.date).toLocaleDateString()}</td>
                    <td>{record.description}</td>
                    <td>{record.provider}</td>
                    <td>${record.cost.toFixed(2)}</td>
                    <td>
                      <span className={`status-badge status-${record.status}`}>
                        {record.status}
                      </span>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="empty-state">No maintenance records found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Maintenance;
