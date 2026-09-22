import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { vehicleService } from '../../services/vehicleService';
import { VehicleDto, PagedResult } from '../../types';
import { Plus, Pencil, Trash2, Car } from 'lucide-react';
import { getStatusBadge } from '../../utils/badgeUtils';

const Vehicles = () => {
  const [data, setData] = useState<PagedResult<VehicleDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
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

  return (
    <div>
      {/* Page Header */}
      <div className="page-header">
        <div className="page-header-left">
          <h1>Vehicles</h1>
          <p>Manage your fleet of {data?.totalCount ?? 0} vehicles</p>
        </div>
        <button className="btn btn-primary" onClick={() => navigate('/vehicles/new')}>
          <Plus size={16} /> Add Vehicle
        </button>
      </div>

      {/* Table Card */}
      <div className="table-card">
        <div className="table-card-header">
          <div>
            <div className="table-card-title">All Vehicles</div>
            <div className="table-card-subtitle">{data?.totalCount ?? 0} records found</div>
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
                {data?.items && data.items.length > 0 ? (
                  data.items.map(vehicle => (
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
                      <td style={{ color: 'var(--text-muted)', fontSize: '0.8125rem' }}>{vehicle.vehicleType}</td>
                      <td><span className={getStatusBadge(String(vehicle.status))}>{vehicle.status}</span></td>
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
                  <tr><td colSpan={7} className="state-container" style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>No vehicles found. Add your first vehicle.</td></tr>
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
