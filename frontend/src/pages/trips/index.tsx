import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { tripService } from '../../services/tripService';
import { TripDto, PagedResult } from '../../types';
import { Plus, Search, Filter, Trash2 } from 'lucide-react';
import '../vehicles/Vehicles.css'; // Reusing base table styles

const Trips = () => {
  const [data, setData] = useState<PagedResult<TripDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

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

  const deleteTrip = async (trip: TripDto) => {
    if (!window.confirm(`Delete trip ${trip.tripNumber}? Completed and cancelled trips cannot be deleted.`)) return;
    try { await tripService.deleteTrip(trip.id); await fetchTrips(); } catch (err: any) { setError(err.response?.data?.message || 'Could not delete trip.'); }
  };

  return (
    <div className="vehicles-container fade-in">
      <div className="page-header">
        <h1>Trips Management</h1>
        <button className="btn btn-primary" onClick={() => navigate('/trips/new')}>
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
                <th>Trip #</th>
                <th>Vehicle</th>
                <th>Driver</th>
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
                    <td><strong>{trip.tripNumber}</strong></td>
                    <td>{trip.vehicleRegistrationNumber}</td>
                    <td>{trip.driverName}</td>
                    <td>{trip.startLocation} &rarr; {trip.destination}</td>
                    <td>{trip.startTime ? new Date(trip.startTime).toLocaleString() : 'Not started'}</td>
                    <td>
                      <span className={`status-badge status-${trip.status.toLowerCase().replace(/\s+/g, '-')}`}>
                        {trip.status}
                      </span>
                    </td>
                    <td>
                      <Link className="action-btn" to={`/trips/${trip.id}/edit`}>Edit</Link>{' '}
                      <button className="action-btn" onClick={() => deleteTrip(trip)} title="Delete trip"><Trash2 size={14} /></button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={7} className="empty-state">No trips found.</td>
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
