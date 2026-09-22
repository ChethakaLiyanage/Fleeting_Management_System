import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { incidentService } from '../../services/incidentService';
import { IncidentDto, PagedResult } from '../../types';
import { Plus, Search, Trash2 } from 'lucide-react';
import '../vehicles/Vehicles.css';

const Incidents = () => {
  const [data, setData] = useState<PagedResult<IncidentDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

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

  const deleteIncident = async (incident: IncidentDto) => {
    if (!window.confirm(`Delete incident for ${incident.vehicleRegistration || 'this vehicle'}?`)) return;
    try { await incidentService.deleteIncident(incident.id); await fetchIncidents(); } catch (err: any) { setError(err.response?.data?.message || 'Could not delete incident.'); }
  };

  return (
    <div className="vehicles-container fade-in">
      <div className="page-header">
        <h1>Incident Reports</h1>
        <button className="btn btn-primary" onClick={() => navigate('/incidents/new')}>
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
                <th>Vehicle</th>
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
                    <td><strong>{incident.vehicleRegistration || 'N/A'}</strong></td>
                    <td>{new Date(incident.date).toLocaleDateString()}</td>
                    <td>{incident.description}</td>
                    <td>
                      <span className={`status-badge status-${incident.severity}`}>
                        {incident.severity}
                      </span>
                    </td>
                    <td>{incident.status}</td>
                    <td>
                      <Link className="action-btn" to={`/incidents/${incident.id}/edit`}>Edit</Link>{' '}
                      <button className="action-btn" onClick={() => deleteIncident(incident)} title="Delete incident"><Trash2 size={14} /></button>
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
