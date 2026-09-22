import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { inspectionService } from '../../services/inspectionService';
import { InspectionDto, PagedResult } from '../../types';
import { Plus, Search, Trash2 } from 'lucide-react';
import '../vehicles/Vehicles.css';

const Inspections = () => {
  const [data, setData] = useState<PagedResult<InspectionDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

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

  const deleteInspection = async (inspection: InspectionDto) => {
    if (!window.confirm(`Delete inspection for ${inspection.vehicleRegistrationNumber}?`)) return;
    try { await inspectionService.deleteInspection(inspection.id); await fetchInspections(); } catch (err: any) { setError(err.response?.data?.message || 'Could not delete inspection.'); }
  };

  return (
    <div className="vehicles-container fade-in">
      <div className="page-header">
        <h1>Vehicle Inspections</h1>
        <button className="btn btn-primary" onClick={() => navigate('/inspections/new')}>
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
                <th>Vehicle</th>
                <th>Date</th>
                <th>Inspector / Driver</th>
                <th>Type</th>
                <th>Result</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map(inspection => (
                  <tr key={inspection.id}>
                    <td><strong>{inspection.vehicleRegistrationNumber}</strong></td>
                    <td>{new Date(inspection.inspectionDate).toLocaleDateString()}</td>
                    <td>{inspection.driverName || 'N/A'}</td>
                    <td>{inspection.type}</td>
                    <td>
                      <span className={`status-badge status-${inspection.result.toLowerCase().replace(/\s+/g, '-')}`}>
                        {inspection.result}
                      </span>
                    </td>
                    <td>
                      <Link className="action-btn" to={`/inspections/${inspection.id}/edit`}>Edit</Link>{' '}
                      <button className="action-btn" onClick={() => deleteInspection(inspection)} title="Delete inspection"><Trash2 size={14} /></button>
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
