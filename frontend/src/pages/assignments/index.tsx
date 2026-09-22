import { useEffect, useState } from 'react';
import { assignmentService } from '../../services/assignmentService';
import { VehicleAssignmentDto, PagedResult } from '../../types';
import { Plus, Search, Filter } from 'lucide-react';
import '../vehicles/Vehicles.css';

const Assignments = () => {
  const [data, setData] = useState<PagedResult<VehicleAssignmentDto> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchAssignments();
  }, []);

  const fetchAssignments = async () => {
    try {
      setLoading(true);
      const res = await assignmentService.getAssignments();
      if (res.success && res.data) {
        setData(res.data);
      } else {
        setError(res.message || 'Failed to load assignments');
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
        <h1>Vehicle Assignments</h1>
        <button className="btn btn-primary">
          <Plus size={18} /> New Assignment
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search assignments..." />
        </div>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading assignments...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchAssignments}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Vehicle ID</th>
                <th>Driver ID</th>
                <th>Start Date</th>
                <th>End Date</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data?.items && data.items.length > 0 ? (
                data.items.map(assignment => (
                  <tr key={assignment.id}>
                    <td><strong>{assignment.vehicleId}</strong></td>
                    <td>{assignment.driverId}</td>
                    <td>{new Date(assignment.startDate).toLocaleDateString()}</td>
                    <td>{assignment.endDate ? new Date(assignment.endDate).toLocaleDateString() : 'N/A'}</td>
                    <td>
                      <span className={`status-badge status-${assignment.status}`}>
                        {assignment.status}
                      </span>
                    </td>
                    <td>
                      <button className="action-btn">Revoke</button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={6} className="empty-state">No active assignments found.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Assignments;
