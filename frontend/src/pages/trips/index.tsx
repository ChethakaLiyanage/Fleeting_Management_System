import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { tripService } from '../../services/tripService';
import { TripDto, PagedResult } from '../../types';
import { Plus, Pencil, Trash2, Map, Search } from 'lucide-react';
import { getStatusBadge } from '../../utils/badgeUtils';

const Trips = () => {
  const [data, setData] = useState<PagedResult<TripDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [query, setQuery] = useState('');
  const navigate = useNavigate();

  useEffect(() => { fetchTrips(); }, []);

  const fetchTrips = async () => {
    try {
      setLoading(true);
      const res = await tripService.getTrips();
      if (res.success && res.data) setData(res.data);
      else setError(res.message || 'Failed to load trips');
    } catch { setError('Network error: Could not reach the server'); }
    finally { setLoading(false); }
  };

  const deleteTrip = async (trip: TripDto) => {
    if (!window.confirm(`Delete trip ${trip.tripNumber}?`)) return;
    try { await tripService.deleteTrip(trip.id); await fetchTrips(); }
    catch (err: any) { setError(err.response?.data?.message || 'Could not delete trip.'); }
  };

  const q = query.toLowerCase();
  const filtered = (data?.items ?? []).filter(t =>
    !q ||
    t.tripNumber?.toLowerCase().includes(q) ||
    t.vehicleRegistrationNumber?.toLowerCase().includes(q) ||
    t.driverName?.toLowerCase().includes(q) ||
    t.startLocation?.toLowerCase().includes(q) ||
    t.destination?.toLowerCase().includes(q) ||
    String(t.status)?.toLowerCase().includes(q)
  );

  return (
    <div>
      <div className="page-header">
        <div className="page-header-left">
          <h1>Trips</h1>
          <p>Track and manage all fleet trip records</p>
        </div>
        <button className="btn btn-primary" onClick={() => navigate('/trips/new')}>
          <Plus size={16} /> Log Trip
        </button>
      </div>

      <div className="table-card">
        <div className="table-card-header">
          <div>
            <div className="table-card-title">All Trips</div>
            <div className="table-card-subtitle">{filtered.length} of {data?.totalCount ?? 0} records</div>
          </div>
          <div className="table-search-wrap">
            <Search size={15} className="table-search-icon" />
            <input
              type="text"
              className="table-search-input"
              placeholder="Search by trip #, vehicle, driver, route..."
              value={query}
              onChange={e => setQuery(e.target.value)}
            />
          </div>
        </div>

        {loading ? (
          <div className="state-container"><span className="spinner" style={{ border: '2px solid #e5e7eb', borderTop: '2px solid var(--primary)' }} /><p>Loading trips...</p></div>
        ) : error ? (
          <div className="state-container" style={{ color: 'var(--danger)' }}>
            <p>{error}</p>
            <button className="btn btn-secondary" style={{ marginTop: '1rem' }} onClick={fetchTrips}>Retry</button>
          </div>
        ) : (
          <div className="table-responsive">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Trip #</th>
                  <th>Vehicle</th>
                  <th>Driver</th>
                  <th>Route</th>
                  <th>Scheduled</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.length > 0 ? (
                  filtered.map(trip => (
                    <tr key={trip.id}>
                      <td>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                          <div style={{ width: 28, height: 28, background: 'var(--accent-light)', borderRadius: 6, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                            <Map size={13} color="#b45309" />
                          </div>
                          <strong style={{ fontFamily: 'monospace', fontSize: '0.8125rem' }}>{trip.tripNumber}</strong>
                        </div>
                      </td>
                      <td style={{ fontSize: '0.8125rem' }}>{trip.vehicleRegistrationNumber}</td>
                      <td style={{ fontSize: '0.8125rem' }}>{trip.driverName}</td>
                      <td style={{ fontSize: '0.8125rem', color: 'var(--text-muted)', maxWidth: 180, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {trip.startLocation} → {trip.destination}
                      </td>
                      <td style={{ fontSize: '0.8125rem', color: 'var(--text-muted)' }}>
                        {trip.startTime ? new Date(trip.startTime).toLocaleDateString() : '—'}
                      </td>
                      <td><span className={getStatusBadge(trip.status)}>{trip.status}</span></td>
                      <td>
                        <div className="action-btns">
                          <button className="btn-icon" title="Edit" onClick={() => navigate(`/trips/${trip.id}/edit`)}><Pencil size={13} /></button>
                          <button className="btn-icon danger" title="Delete" onClick={() => deleteTrip(trip)}><Trash2 size={13} /></button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr><td colSpan={7}>
                    <div className="state-container"><Map size={32} style={{ opacity: 0.3 }} /><p>{query ? `No trips matching "${query}".` : 'No trips found. Log your first trip.'}</p></div>
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

export default Trips;
