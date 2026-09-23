import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { driverService } from '../../services/driverService';
import { DriverDto, PagedResult } from '../../types';
import { Plus, Pencil, Trash2, Users, AlertCircle, Search } from 'lucide-react';
import { getStatusBadge } from '../../utils/badgeUtils';

const getDriverStatusLabel = (status: number | string): string => {
  switch (Number(status)) {
    case 1: return 'Available';
    case 2: return 'Assigned';
    case 3: return 'On Trip';
    case 4: return 'Leave';
    case 5: return 'Suspended';
    case 6: return 'Inactive';
    default: return String(status);
  }
};

const Drivers = () => {
  const [data, setData] = useState<PagedResult<DriverDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [query, setQuery] = useState('');
  const navigate = useNavigate();

  useEffect(() => { fetchDrivers(); }, []);

  const fetchDrivers = async () => {
    try {
      setLoading(true);
      const res = await driverService.getDrivers();
      if (res.success && res.data) setData(res.data);
      else setError(res.message || 'Failed to load drivers');
    } catch { setError('Network error: Could not reach the server'); }
    finally { setLoading(false); }
  };

  const deleteDriver = async (driver: DriverDto) => {
    if (!window.confirm(`Deactivate driver ${driver.fullName}?`)) return;
    try { await driverService.deleteDriver(driver.id); await fetchDrivers(); }
    catch (err: any) { setError(err.response?.data?.message || 'Could not deactivate driver.'); }
  };

  const q = query.toLowerCase();
  const filtered = (data?.items ?? []).filter(d =>
    !q ||
    d.fullName?.toLowerCase().includes(q) ||
    d.email?.toLowerCase().includes(q) ||
    d.employeeNumber?.toLowerCase().includes(q) ||
    d.licenseNumber?.toLowerCase().includes(q) ||
    getDriverStatusLabel(d.status).toLowerCase().includes(q)
  );

  return (
    <div>
      <div className="page-header">
        <div className="page-header-left">
          <h1>Drivers</h1>
          <p>Manage your fleet drivers and licenses</p>
        </div>
        <button className="btn btn-primary" onClick={() => navigate('/drivers/new')}>
          <Plus size={16} /> Add Driver
        </button>
      </div>

      <div className="table-card">
        <div className="table-card-header">
          <div>
            <div className="table-card-title">All Drivers</div>
            <div className="table-card-subtitle">{filtered.length} of {data?.totalCount ?? 0} records</div>
          </div>
          <div className="table-search-wrap">
            <Search size={15} className="table-search-icon" />
            <input
              type="text"
              className="table-search-input"
              placeholder="Search by name, email, employee #..."
              value={query}
              onChange={e => setQuery(e.target.value)}
            />
          </div>
        </div>

        {loading ? (
          <div className="state-container"><span className="spinner" style={{ border: '2px solid #e5e7eb', borderTop: '2px solid var(--primary)' }} /><p>Loading drivers...</p></div>
        ) : error ? (
          <div className="state-container" style={{ color: 'var(--danger)' }}>
            <AlertCircle size={24} /><p>{error}</p>
            <button className="btn btn-secondary" style={{ marginTop: '1rem' }} onClick={fetchDrivers}>Retry</button>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Driver</th>
                  <th>Employee #</th>
                  <th>License No.</th>
                  <th>License Expiry</th>
                  <th>Status</th>
                  <th>Phone</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.length > 0 ? (
                  filtered.map(driver => (
                    <tr key={driver.id}>
                      <td>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.625rem' }}>
                          <div style={{ width: 34, height: 34, background: 'linear-gradient(135deg, var(--primary), #7f1d1d)', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white', fontWeight: 700, fontSize: '0.8125rem', flexShrink: 0 }}>
                            {driver.fullName.charAt(0).toUpperCase()}
                          </div>
                          <div>
                            <div style={{ fontWeight: 600, fontSize: '0.875rem' }}>{driver.fullName}</div>
                            <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{driver.email}</div>
                          </div>
                        </div>
                      </td>
                      <td style={{ fontFamily: 'monospace', fontSize: '0.8125rem', color: 'var(--text-muted)' }}>{driver.employeeNumber}</td>
                      <td style={{ fontSize: '0.8125rem' }}>{driver.licenseNumber}</td>
                      <td>
                        {driver.isLicenseExpired
                          ? <span className="badge badge-danger">Expired</span>
                          : <span style={{ fontSize: '0.8125rem' }}>{driver.licenseExpiry ? new Date(driver.licenseExpiry).toLocaleDateString() : '—'}</span>
                        }
                      </td>
                      <td><span className={getStatusBadge(getDriverStatusLabel(driver.status))}>{getDriverStatusLabel(driver.status)}</span></td>
                      <td style={{ fontSize: '0.8125rem' }}>{driver.phone}</td>
                      <td>
                        <div className="action-btns">
                          <button className="btn-icon" title="Edit" onClick={() => navigate(`/drivers/${driver.id}/edit`)}><Pencil size={13} /></button>
                          <button className="btn-icon danger" title="Deactivate" onClick={() => deleteDriver(driver)}><Trash2 size={13} /></button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr><td colSpan={7}>
                    <div className="state-container"><Users size={32} style={{ opacity: 0.3 }} /><p>{query ? `No drivers matching "${query}".` : 'No drivers found. Add your first driver.'}</p></div>
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

export default Drivers;
