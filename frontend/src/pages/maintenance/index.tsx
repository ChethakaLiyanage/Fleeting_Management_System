import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { maintenanceService } from '../../services/maintenanceService';
import { MaintenanceRecordDto } from '../../types';
import { Plus, Pencil, Trash2, Wrench, Search } from 'lucide-react';
import { getStatusBadge } from '../../utils/badgeUtils';

const getTypeLabel = (type: number | string): string => {
  switch (Number(type)) {
    case 0: return 'Preventive';
    case 1: return 'Corrective';
    case 2: return 'Predictive';
    case 3: return 'Routine';
    default: return String(type);
  }
};

const getStatusLabel = (status: number | string): string => {
  switch (Number(status)) {
    case 0: return 'Scheduled';
    case 1: return 'InProgress';
    case 2: return 'Completed';
    case 3: return 'Cancelled';
    default: return String(status);
  }
};

const Maintenance = () => {
  const [data, setData] = useState<MaintenanceRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [query, setQuery] = useState('');
  const navigate = useNavigate();

  useEffect(() => { fetchMaintenance(); }, []);

  const fetchMaintenance = async () => {
    try {
      setLoading(true);
      setError('');
      const records = await maintenanceService.getMaintenanceRecords();
      setData(Array.isArray(records) ? records : []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Network error: Could not reach the server');
    } finally {
      setLoading(false);
    }
  };

  const deleteMaintenance = async (record: MaintenanceRecordDto) => {
    if (!window.confirm(`Delete maintenance record for ${record.vehicleRegistration || 'this vehicle'}?`)) return;
    try { await maintenanceService.deleteMaintenanceRecord(record.id); await fetchMaintenance(); }
    catch (err: any) { setError(err.response?.data?.message || 'Could not delete maintenance record.'); }
  };

  const q = query.toLowerCase();
  const filtered = data.filter(r =>
    !q ||
    r.vehicleRegistration?.toLowerCase().includes(q) ||
    r.description?.toLowerCase().includes(q) ||
    r.serviceProvider?.toLowerCase().includes(q) ||
    getTypeLabel(r.type).toLowerCase().includes(q) ||
    getStatusLabel(r.status).toLowerCase().includes(q)
  );

  return (
    <div>
      <div className="page-header">
        <div className="page-header-left">
          <h1>Maintenance</h1>
          <p>Schedule and track vehicle maintenance records</p>
        </div>
        <button className="btn btn-primary" onClick={() => navigate('/maintenance/new')}>
          <Plus size={16} /> Schedule Maintenance
        </button>
      </div>

      <div className="table-card">
        <div className="table-card-header">
          <div>
            <div className="table-card-title">All Maintenance Records</div>
            <div className="table-card-subtitle">{filtered.length} of {data.length} records</div>
          </div>
          <div className="table-search-wrap">
            <Search size={15} className="table-search-icon" />
            <input
              type="text"
              className="table-search-input"
              placeholder="Search by vehicle, type, provider..."
              value={query}
              onChange={e => setQuery(e.target.value)}
            />
          </div>
        </div>

        {loading ? (
          <div className="state-container"><span className="spinner" style={{ border: '2px solid #e5e7eb', borderTop: '2px solid var(--primary)' }} /><p>Loading records...</p></div>
        ) : error ? (
          <div className="state-container" style={{ color: 'var(--danger)' }}>
            <p>{error}</p>
            <button className="btn btn-secondary" style={{ marginTop: '1rem' }} onClick={fetchMaintenance}>Retry</button>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Vehicle</th>
                  <th>Type</th>
                  <th>Scheduled Date</th>
                  <th>Description</th>
                  <th>Provider</th>
                  <th>Cost</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.length > 0 ? (
                  filtered.map(record => {
                    const statusLabel = getStatusLabel(record.status);
                    return (
                      <tr key={record.id}>
                        <td>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                            <div style={{ width: 28, height: 28, background: 'var(--accent-light)', borderRadius: 6, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                              <Wrench size={13} color="#b45309" />
                            </div>
                            <div>
                              <strong style={{ fontSize: '0.8125rem' }}>{record.vehicleRegistration || 'N/A'}</strong>
                              {record.isOverdue && <div className="badge badge-danger" style={{ display: 'inline-block', marginLeft: 6, fontSize: '0.65rem' }}>Overdue</div>}
                            </div>
                          </div>
                        </td>
                        <td style={{ fontSize: '0.8125rem' }}>{getTypeLabel(record.type)}</td>
                        <td style={{ fontSize: '0.8125rem', color: record.isOverdue ? 'var(--danger)' : 'var(--text-muted)' }}>
                          {new Date(record.scheduledDate).toLocaleDateString()}
                        </td>
                        <td style={{ fontSize: '0.8125rem', maxWidth: 180, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{record.description}</td>
                        <td style={{ fontSize: '0.8125rem', color: 'var(--text-muted)' }}>{record.serviceProvider || '—'}</td>
                        <td style={{ fontWeight: 600, fontSize: '0.875rem' }}>${(record.cost ?? 0).toFixed(2)}</td>
                        <td><span className={getStatusBadge(statusLabel)}>{statusLabel}</span></td>
                        <td>
                          <div className="action-btns">
                            <button className="btn-icon" title="Edit" onClick={() => navigate(`/maintenance/${record.id}/edit`)}><Pencil size={13} /></button>
                            <button className="btn-icon danger" title="Delete" onClick={() => deleteMaintenance(record)}><Trash2 size={13} /></button>
                          </div>
                        </td>
                      </tr>
                    );
                  })
                ) : (
                  <tr><td colSpan={8}>
                    <div className="state-container"><Wrench size={32} style={{ opacity: 0.3 }} /><p>{query ? `No maintenance records matching "${query}".` : 'No maintenance records found. Schedule your first service.'}</p></div>
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

export default Maintenance;
