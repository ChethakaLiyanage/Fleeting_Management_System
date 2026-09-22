import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { incidentService } from '../../services/incidentService';
import { IncidentDto, PagedResult } from '../../types';
import { Plus, Pencil, Trash2, AlertTriangle } from 'lucide-react';
import { getStatusBadge } from '../../utils/badgeUtils';

const Incidents = () => {
  const [data, setData] = useState<PagedResult<IncidentDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => { fetchIncidents(); }, []);

  const fetchIncidents = async () => {
    try {
      setLoading(true);
      const res = await incidentService.getIncidents();
      if (res.success && res.data) setData(res.data);
      else setError(res.message || 'Failed to load incidents');
    } catch { setError('Network error: Could not reach the server'); }
    finally { setLoading(false); }
  };

  const deleteIncident = async (incident: IncidentDto) => {
    if (!window.confirm(`Delete incident for ${incident.vehicleRegistration || 'this vehicle'}?`)) return;
    try { await incidentService.deleteIncident(incident.id); await fetchIncidents(); }
    catch (err: any) { setError(err.response?.data?.message || 'Could not delete incident.'); }
  };

  return (
    <div>
      <div className="page-header">
        <div className="page-header-left">
          <h1>Incidents</h1>
          <p>Report and track vehicle incidents and accidents</p>
        </div>
        <button className="btn btn-primary" onClick={() => navigate('/incidents/new')}>
          <Plus size={16} /> Report Incident
        </button>
      </div>

      <div className="table-card">
        <div className="table-card-header">
          <div>
            <div className="table-card-title">All Incidents</div>
            <div className="table-card-subtitle">{data?.totalCount ?? 0} records found</div>
          </div>
        </div>

        {loading ? (
          <div className="state-container"><span className="spinner" style={{ border: '2px solid #e5e7eb', borderTop: '2px solid var(--primary)' }} /><p>Loading incidents...</p></div>
        ) : error ? (
          <div className="state-container" style={{ color: 'var(--danger)' }}>
            <p>{error}</p>
            <button className="btn btn-secondary" style={{ marginTop: '1rem' }} onClick={fetchIncidents}>Retry</button>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Vehicle</th>
                  <th>Date</th>
                  <th>Type</th>
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
                      <td>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                          <div style={{ width: 28, height: 28, background: '#fee2e2', borderRadius: 6, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            <AlertTriangle size={13} color="var(--danger)" />
                          </div>
                          <strong style={{ fontSize: '0.8125rem' }}>{incident.vehicleRegistration || 'N/A'}</strong>
                        </div>
                      </td>
                      <td style={{ fontSize: '0.8125rem', color: 'var(--text-muted)' }}>{new Date(incident.date).toLocaleDateString()}</td>
                      <td style={{ fontSize: '0.8125rem' }}>{incident.type}</td>
                      <td style={{ fontSize: '0.8125rem', maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{incident.description}</td>
                      <td><span className={getStatusBadge(String(incident.severity))}>{incident.severity}</span></td>
                      <td><span className={getStatusBadge(String(incident.status))}>{incident.status}</span></td>
                      <td>
                        <div className="action-btns">
                          <button className="btn-icon" title="Edit" onClick={() => navigate(`/incidents/${incident.id}/edit`)}><Pencil size={13} /></button>
                          <button className="btn-icon danger" title="Delete" onClick={() => deleteIncident(incident)}><Trash2 size={13} /></button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr><td colSpan={7}>
                    <div className="state-container"><AlertTriangle size={32} style={{ opacity: 0.3 }} /><p>No incidents found. All clear!</p></div>
                  </td></tr>
                )}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};

export default Incidents;
