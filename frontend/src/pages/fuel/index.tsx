import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { fuelService } from '../../services/fuelService';
import { FuelRecordDto } from '../../types';
import { Plus, Pencil, Trash2, Droplet } from 'lucide-react';

const Fuel = () => {
  const [data, setData] = useState<FuelRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => { fetchFuel(); }, []);

  const fetchFuel = async () => {
    try {
      setLoading(true);
      setError('');
      const records = await fuelService.getFuelRecords();
      setData(Array.isArray(records) ? records : []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Network error: Could not reach the server');
    } finally {
      setLoading(false);
    }
  };

  const deleteFuel = async (log: FuelRecordDto) => {
    if (!window.confirm(`Delete fuel log for ${log.vehicleRegistration || 'this vehicle'}?`)) return;
    try { await fuelService.deleteFuelRecord(log.id); await fetchFuel(); }
    catch (err: any) { setError(err.response?.data?.message || 'Could not delete fuel log.'); }
  };

  return (
    <div>
      <div className="page-header">
        <div className="page-header-left">
          <h1>Fuel Records</h1>
          <p>Track fuel consumption across your fleet</p>
        </div>
        <button className="btn btn-primary" onClick={() => navigate('/fuel/new')}>
          <Plus size={16} /> Log Fuel
        </button>
      </div>

      <div className="table-card">
        <div className="table-card-header">
          <div>
            <div className="table-card-title">All Fuel Logs</div>
            <div className="table-card-subtitle">{data.length} records found</div>
          </div>
        </div>

        {loading ? (
          <div className="state-container"><span className="spinner" style={{ border: '2px solid #e5e7eb', borderTop: '2px solid var(--primary)' }} /><p>Loading fuel records...</p></div>
        ) : error ? (
          <div className="state-container" style={{ color: 'var(--danger)' }}>
            <p>{error}</p>
            <button className="btn btn-secondary" style={{ marginTop: '1rem' }} onClick={fetchFuel}>Retry</button>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Vehicle</th>
                  <th>Driver</th>
                  <th>Date</th>
                  <th>Fuel Type</th>
                  <th>Volume (L)</th>
                  <th>Cost / L</th>
                  <th>Total Cost</th>
                  <th>Station</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {data.length > 0 ? (
                  data.map(log => (
                    <tr key={log.id}>
                      <td>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                          <div style={{ width: 28, height: 28, background: 'var(--info-light)', borderRadius: 6, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            <Droplet size={13} color="#1d4ed8" />
                          </div>
                          <strong style={{ fontSize: '0.8125rem' }}>{log.vehicleRegistration || 'N/A'}</strong>
                        </div>
                      </td>
                      <td style={{ fontSize: '0.8125rem' }}>{log.driverName || '—'}</td>
                      <td style={{ fontSize: '0.8125rem', color: 'var(--text-muted)' }}>{new Date(log.fuelDate).toLocaleDateString()}</td>
                      <td>
                        <span className="badge badge-info" style={{ fontSize: '0.7rem' }}>{log.fuelType}</span>
                      </td>
                      <td style={{ fontSize: '0.875rem', fontWeight: 500 }}>{log.litres} L</td>
                      <td style={{ fontSize: '0.8125rem', color: 'var(--text-muted)' }}>${log.costPerLitre.toFixed(2)}</td>
                      <td>
                        <span style={{ fontWeight: 600, color: 'var(--primary)', fontSize: '0.875rem' }}>${log.totalCost.toFixed(2)}</span>
                      </td>
                      <td style={{ fontSize: '0.8125rem', color: 'var(--text-muted)' }}>{log.station || '—'}</td>
                      <td>
                        <div className="action-btns">
                          <button className="btn-icon" title="Edit" onClick={() => navigate(`/fuel/${log.id}/edit`)}><Pencil size={13} /></button>
                          <button className="btn-icon danger" title="Delete" onClick={() => deleteFuel(log)}><Trash2 size={13} /></button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr><td colSpan={9}>
                    <div className="state-container"><Droplet size={32} style={{ opacity: 0.3 }} /><p>No fuel records found. Log your first fuel entry.</p></div>
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

export default Fuel;
