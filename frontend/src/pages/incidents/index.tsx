import { useEffect, useState } from 'react';
import { incidentService } from '../../services/incidentService';
import { IncidentDto, PagedResult } from '../../types';
import { Plus, Search, Filter } from 'lucide-react';
import '../vehicles/Vehicles.css';

const Incidents = () => {
  const [data, setData] = useState<PagedResult<IncidentDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchIncidents();
  }, []);

  const fetchIncidents = async () => {
    try {
      setLoading(true);
      const res = await incidentService.getIncidents();
      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load incidents');
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
        <h1>Incident Reports</h1>
        <button className="btn btn-primary">
          <Plus size={18} /> Report Incident
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search incidents..." />
        </div>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading incidents...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchIncidents}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Vehicle ID</th>
                <th>Date</th>
                <th>Description</th>
                <th>Severity</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map(incident => (
                  <tr key={incident.id}>
                    <td><strong>{incident.vehicleId}</strong></td>
                    <td>{new Date(incident.date).toLocaleDateString()}</td>
                    <td>{incident.description}</td>
                    <td>
                      <span className={`status-badge status-${incident.severity}`}>
                        {incident.severity}
                      </span>
                    </td>
                    <td>{incident.status}</td>
                    <td>
                      <button className="action-btn">View</button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="empty-state">No incidents found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Incidents;
