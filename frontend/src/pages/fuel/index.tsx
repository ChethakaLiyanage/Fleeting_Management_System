import { useEffect, useState } from 'react';
import { fuelService } from '../../services/fuelService';
import { FuelRecordDto, PagedResult } from '../../types';
import { Plus, Search, Filter } from 'lucide-react';
import '../vehicles/Vehicles.css';

const Fuel = () => {
  const [data, setData] = useState<PagedResult<FuelRecordDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchFuel();
  }, []);

  const fetchFuel = async () => {
    try {
      setLoading(true);
      const res = await fuelService.getFuelRecords();
      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load fuel records');
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
        <h1>Fuel Logs</h1>
        <button className="btn btn-primary">
          <Plus size={18} /> Add Fuel Log
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search logs..." />
        </div>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading fuel logs...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchFuel}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Vehicle ID</th>
                <th>Date</th>
                <th>Volume (L)</th>
                <th>Cost</th>
                <th>Location</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map(log => (
                  <tr key={log.id}>
                    <td><strong>{log.vehicleId}</strong></td>
                    <td>{new Date(log.date).toLocaleDateString()}</td>
                    <td>{log.volume}</td>
                    <td>${log.cost.toFixed(2)}</td>
                    <td>{log.location}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className="empty-state">No fuel logs found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Fuel;
