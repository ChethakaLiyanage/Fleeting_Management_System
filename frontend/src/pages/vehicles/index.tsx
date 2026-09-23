import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { vehicleService } from '../../services/vehicleService';
import { VehicleDto, PagedResult } from '../../types';
import { Plus, Pencil, Trash2, Car, Search } from 'lucide-react';
import { getStatusBadge } from '../../utils/badgeUtils';

const getVehicleStatusLabel = (status: number | string): string => {
  switch (Number(status)) {
    case 1: return 'Available';
    case 2: return 'Assigned';
    case 3: return 'On Trip';
    case 4: return 'Maintenance';
    case 5: return 'Out of Service';
    case 6: return 'Retired';
    default: return String(status);
  }
};

const getVehicleTypeLabel = (type: number | string): string => {
  switch (Number(type)) {
    case 1: return 'Sedan';
    case 2: return 'SUV';
    case 3: return 'Truck';
    case 4: return 'Van';
    case 5: return 'Bus';
    case 6: return 'Motorcycle';
    case 7: return 'Other';
    default: return String(type);
  }
};

const Vehicles = () => {
  const [data, setData] = useState<PagedResult<VehicleDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [query, setQuery] = useState('');
  const navigate = useNavigate();

  useEffect(() => { fetchVehicles(); }, []);

  const fetchVehicles = async () => {
    try {
      setLoading(true);
      const res = await vehicleService.getVehicles();
      if (res.success && res.data) setData(res.data);
      else setError(res.message || 'Failed to load vehicles');
    } catch { setError('Network error: Could not reach the server'); }
    finally { setLoading(false); }
  };

  const deleteVehicle = async (vehicle: VehicleDto) => {
    if (!window.confirm(`Archive vehicle ${vehicle.registrationNumber}?`)) return;
    try { await vehicleService.deleteVehicle(vehicle.id); await fetchVehicles(); }
    catch (err: any) { setError(err.response?.data?.message || 'Could not archive vehicle.'); }
  };

  const q = query.toLowerCase();
  const filtered = (data?.items ?? []).filter(v =>
    !q ||
    v.registrationNumber?.toLowerCase().includes(q) ||
    v.make?.toLowerCase().includes(q) ||
    v.model?.toLowerCase().includes(q) ||
    getVehicleTypeLabel(v.vehicleType).toLowerCase().includes(q) ||
    getVehicleStatusLabel(v.status).toLowerCase().includes(q)
  );

  return (
    <div>
      <div className="page-header">
        <div className="page-header-left">
          <h1>Vehicles</h1>
          <p>Manage your fleet of {data?.totalCount ?? 0} vehicles</p>
        </div>
        <button className="btn btn-primary" onClick={() => navigate('/vehicles/new')}>
          <Plus size={16} /> Add Vehicle
        </button>
      </div>

      <div className="table-card">
        <div className="table-card-header">
          <div>
            <div className="table-card-title">All Vehicles</div>
            <div className="table-card-subtitle">{filtered.length} of {data?.totalCount ?? 0} records</div>
          </div>
          <div className="table-search-wrap">
            <Search size={15} className="table-search-icon" />
            <input
              type="text"
              className="table-search-input"
              placeholder="Search by registration, make, model..."
              value={query}
              onChange={e => setQuery(e.target.value)}
            />
          </div>
        </div>

        {loading ? (
          <div className="state-container"><span className="spinner" style={{ border: '2px solid #e5e7eb', borderTop: '2px solid var(--primary)' }} /><p>Loading vehicles...</p></div>
        ) : error ? (
          <div className="state-container" style={{ color: 'var(--danger)' }}>
            <p>{error}</p>
            <button className="btn btn-secondary" style={{ marginTop: '1rem' }} onClick={fetchVehicles}>Retry</button>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Registration</th>
                  <th>Make &amp; Model</th>
                  <th>Year</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Mileage</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.length > 0 ? (
                  filtered.map(vehicle => (
                    <tr key={vehicle.id}>
                      <td>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.625rem' }}>
                          <div style={{ width: 32, height: 32, background: 'var(--primary-light)', borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            <Car size={15} color="var(--primary)" />
                          </div>
                          <strong>{vehicle.registrationNumber}</strong>
                        </div>
                      </td>
                      <td>{vehicle.make} {vehicle.model}</td>
                      <td>{vehicle.year}</td>
                      <td style={{ color: 'var(--text-muted)', fontSize: '0.8125rem' }}>{getVehicleTypeLabel(vehicle.vehicleType)}</td>
                      <td><span className={getStatusBadge(getVehicleStatusLabel(vehicle.status))}>{getVehicleStatusLabel(vehicle.status)}</span></td>
                      <td>{vehicle.mileage.toLocaleString()} km</td>
                      <td>
                        <div className="action-btns">
                          <button className="btn-icon" title="Edit" onClick={() => navigate(`/vehicles/${vehicle.id}/edit`)}><Pencil size={13} /></button>
                          <button className="btn-icon danger" title="Archive" onClick={() => deleteVehicle(vehicle)}><Trash2 size={13} /></button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr><td colSpan={7} style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>
                    {query ? `No vehicles matching "${query}".` : 'No vehicles found. Add your first vehicle.'}
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

export default Vehicles;

