import { useEffect, useState } from 'react';
import { tripService } from '../../services/tripService';
import { TripDto, PagedResult } from '../../types';
import { Plus, Search, Filter } from 'lucide-react';
import '../vehicles/Vehicles.css'; // Reusing base table styles

const Trips = () => {
  const [data, setData] = useState<PagedResult<TripDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchTrips();
  }, []);

  const fetchTrips = async () => {
    try {
      setLoading(true);
      const res = await tripService.getTrips();
      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load trips');
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
        <h1>Trips Management</h1>
        <button className="btn btn-primary">
          <Plus size={18} /> Log Trip
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search trips..." />
        </div>
        <button className="btn filter-btn">
          <Filter size={18} /> Filters
        </button>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading trips...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchTrips}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Vehicle ID</th>
                <th>Driver ID</th>
                <th>Route</th>
                <th>Start Time</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map(trip => (
                  <tr key={trip.id}>
                    <td><strong>{trip.vehicleId}</strong></td>
                    <td>{trip.driverId}</td>
                    <td>{trip.startLocation} &rarr; {trip.endLocation}</td>
                    <td>{new Date(trip.startTime).toLocaleString()}</td>
                    <td>
                      <span className={`status-badge status-${trip.status}`}>
                        {trip.status}
                      </span>
                    </td>
                    <td>
                      <button className="action-btn">Details</button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="empty-state">No trips found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Trips;
