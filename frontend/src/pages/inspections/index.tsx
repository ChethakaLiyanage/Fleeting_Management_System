import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { inspectionService } from '../../services/inspectionService';
import { InspectionDto, PagedResult } from '../../types';
import { Plus, Pencil, Trash2, FileCheck } from 'lucide-react';
import { getStatusBadge } from '../../utils/badgeUtils';

const Inspections = () => {
  const [data, setData] = useState<PagedResult<InspectionDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => { fetchInspections(); }, []);

  const fetchInspections = async () => {
    try {
      setLoading(true);
      const res = await inspectionService.getInspections();
      if (res.success && res.data) setData(res.data);
      else setError(res.message || 'Failed to load inspections');
    } catch { setError('Network error: Could not reach the server'); }
    finally { setLoading(false); }
  };

  const deleteInspection = async (inspection: InspectionDto) => {
    if (!window.confirm(`Delete inspection for ${inspection.vehicleRegistrationNumber}?`)) return;
    try { await inspectionService.deleteInspection(inspection.id); await fetchInspections(); }
    catch (err: any) { setError(err.response?.data?.message || 'Could not delete inspection.'); }
  };

  return (
    <div>
      <div className="page-header">
        <div className="page-header-left">
          <h1>Inspections</h1>
          <p>Record and review vehicle inspection reports</p>
        </div>
        <button className="btn btn-primary" onClick={() => navigate('/inspections/new')}>
          <Plus size={16} /> New Inspection
        </button>
      </div>

      <div className="table-card">
        <div className="table-card-header">
          <div>
            <div className="table-card-title">All Inspections</div>
            <div className="table-card-subtitle">{data?.totalCount ?? 0} records found</div>
          </div>
        </div>

        {loading ? (
          <div className="state-container"><span className="spinner" style={{ border: '2px solid #e5e7eb', borderTop: '2px solid var(--primary)' }} /><p>Loading inspections...</p></div>
        ) : error ? (
          <div className="state-container" style={{ color: 'var(--danger)' }}>
            <p>{error}</p>
            <button className="btn btn-secondary" style={{ marginTop: '1rem' }} onClick={fetchInspections}>Retry</button>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Vehicle</th>
                  <th>Date</th>
                  <th>Inspector / Driver</th>
                  <th>Type</th>
                  <th>Result</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {data?.items && data.items.length > 0 ? (
                  data.items.map(inspection => (
                    <tr key={inspection.id}>
                      <td>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                          <div style={{ width: 28, height: 28, background: 'var(--success-light)', borderRadius: 6, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            <FileCheck size={13} color="var(--success)" />
                          </div>
                          <strong style={{ fontSize: '0.8125rem' }}>{inspection.vehicleRegistrationNumber}</strong>
                        </div>
                      </td>
                      <td style={{ fontSize: '0.8125rem', color: 'var(--text-muted)' }}>{new Date(inspection.inspectionDate).toLocaleDateString()}</td>
                      <td style={{ fontSize: '0.8125rem' }}>{inspection.driverName || '—'}</td>
                      <td style={{ fontSize: '0.8125rem' }}>{inspection.type}</td>
                      <td><span className={getStatusBadge(inspection.result)}>{inspection.result}</span></td>
                      <td>
                        <div className="action-btns">
                          <button className="btn-icon" title="Edit" onClick={() => navigate(`/inspections/${inspection.id}/edit`)}><Pencil size={13} /></button>
                          <button className="btn-icon danger" title="Delete" onClick={() => deleteInspection(inspection)}><Trash2 size={13} /></button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr><td colSpan={6}>
                    <div className="state-container"><FileCheck size={32} style={{ opacity: 0.3 }} /><p>No inspections found. Record your first inspection.</p></div>
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

export default Inspections;
