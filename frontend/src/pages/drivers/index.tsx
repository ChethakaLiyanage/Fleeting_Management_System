import { useEffect, useState } from 'react';
import { driverService } from '../../services/driverService';
import { DriverDto, PagedResult } from '../../types';
import { Plus, Search, Filter } from 'lucide-react';
import '../vehicles/Vehicles.css'; // Re-use table styles

const Drivers = () => {
  const [data, setData] = useState<PagedResult<DriverDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchDrivers();
  }, []);

  const fetchDrivers = async () => {
    try {
      setLoading(true);
      const res = await driverService.getDrivers();
      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load drivers');
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
        <h1>Drivers Management</h1>
        <button className="btn btn-primary">
          <Plus size={18} /> Add Driver
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search by name or license..." />
        </div>
        <button className="btn filter-btn">
          <Filter size={18} /> Filters
        </button>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading drivers...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchDrivers}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Employee #</th>
                <th>License</th>
                <th>Status</th>
                <th>Contact</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map(driver => (
                  <tr key={driver.id}>
                    <td><strong>{driver.fullName}</strong></td>
                    <td>{driver.employeeNumber}</td>
                    <td>
                      {driver.licenseNumber}
                      {driver.isLicenseExpired && <span style={{color: 'red', marginLeft: '5px', fontSize: '0.8rem'}}>(Expired)</span>}
                    </td>
                    <td>
                      <span className={`status-badge status-${driver.status}`}>
                        {driver.status}
                      </span>
                    </td>
                    <td>{driver.phone}</td>
                    <td>
                      <button className="action-btn">View</button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="empty-state">No drivers found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Drivers;
