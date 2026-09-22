import { useEffect, useState } from 'react';
import { inspectionService } from '../../services/inspectionService';
import { InspectionDto, PagedResult } from '../../types';
import { Plus, Search, Filter } from 'lucide-react';
import '../vehicles/Vehicles.css';

const Inspections = () => {
  const [data, setData] = useState<PagedResult<InspectionDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchInspections();
  }, []);

  const fetchInspections = async () => {
    try {
      setLoading(true);
      const res = await inspectionService.getInspections();
      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load inspections');
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
        <h1>Vehicle Inspections</h1>
        <button className="btn btn-primary">
          <Plus size={18} /> New Inspection
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search inspections..." />
        </div>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading inspections...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchInspections}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Vehicle ID</th>
                <th>Date</th>
                <th>Inspector</th>
                <th>Status</th>
                <th>Result</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map(inspection => (
                  <tr key={inspection.id}>
                    <td><strong>{inspection.vehicleId}</strong></td>
                    <td>{new Date(inspection.date).toLocaleDateString()}</td>
                    <td>{inspection.inspectorId || 'N/A'}</td>
                    <td>{inspection.status}</td>
                    <td>
                      <span className={`status-badge status-${inspection.result}`}>
                        {inspection.result}
                      </span>
                    </td>
                    <td>
                      <button className="action-btn">View Details</button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="empty-state">No inspections found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Inspections;
