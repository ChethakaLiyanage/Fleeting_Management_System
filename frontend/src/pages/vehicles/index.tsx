import { useEffect, useState } from 'react';
import { vehicleService } from '../../services/vehicleService';
import { VehicleDto, PagedResult } from '../../types';
import { Plus, Search, Filter } from 'lucide-react';
import './Vehicles.css';

const Vehicles = () => {
  const [data, setData] = useState<PagedResult<VehicleDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchVehicles();
  }, []);

  const fetchVehicles = async () => {
    try {
      setLoading(true);
      const res = await vehicleService.getVehicles();
      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load vehicles');
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
        <h1>Vehicles Management</h1>
        <button className="btn btn-primary">
          <Plus size={18} /> Add Vehicle
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search by registration or model..." />
        </div>
        <button className="btn filter-btn">
          <Filter size={18} /> Filters
        </button>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading vehicles...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchVehicles}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Registration</th>
                <th>Make & Model</th>
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
                    <td><strong>{vehicle.registrationNumber}</strong></td>
                    <td>{vehicle.make} {vehicle.model} ({vehicle.year})</td>
                    <td>{vehicle.vehicleType}</td>
                    <td>
                      <span className={`status-badge status-${vehicle.status}`}>
                        {vehicle.status}
                      </span>
                    </td>
                    <td>{vehicle.mileage.toLocaleString()} km</td>
                    <td>
                      <button className="action-btn">Edit</button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="empty-state">No vehicles found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Vehicles;
