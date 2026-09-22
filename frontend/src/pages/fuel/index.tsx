import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { fuelService } from '../../services/fuelService';
import { FuelRecordDto } from '../../types';
import { Plus, Search, Trash2 } from 'lucide-react';
import '../vehicles/Vehicles.css';

const Fuel = () => {
  const [data, setData] = useState<FuelRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    fetchFuel();
  }, []);

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
    try {
      await fuelService.deleteFuelRecord(log.id);
      await fetchFuel();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Could not delete fuel log.');
    }
  };

  return (
    <div className="vehicles-container fade-in">
      <div className="page-header">
        <h1>Fuel Logs</h1>
        <button className="btn btn-primary" onClick={() => navigate('/fuel/new')}>
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
                <th>Vehicle</th>
                <th>Driver</th>
                <th>Date</th>
                <th>Volume (L)</th>
                <th>Cost/L</th>
                <th>Total Cost</th>
                <th>Fuel Type</th>
                <th>Station</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.length > 0 ? (
                data.map(log => (
                  <tr key={log.id}>
                    <td><strong>{log.vehicleRegistration || 'N/A'}</strong></td>
                    <td>{log.driverName || 'N/A'}</td>
                    <td>{new Date(log.fuelDate).toLocaleDateString()}</td>
                    <td>{log.litres}</td>
                    <td>${log.costPerLitre.toFixed(2)}</td>
                    <td><strong>${log.totalCost.toFixed(2)}</strong></td>
                    <td>{log.fuelType}</td>
                    <td>{log.station || 'N/A'}</td>
                    <td>
                      <Link className="action-btn" to={`/fuel/${log.id}/edit`}>Edit</Link>{' '}
                      <button className="action-btn" onClick={() => deleteFuel(log)} title="Delete fuel log"><Trash2 size={14} /></button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={9} className="empty-state">No fuel logs found. Click "Add Fuel Log" to create one.</td>
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
