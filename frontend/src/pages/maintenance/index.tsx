import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { maintenanceService } from '../../services/maintenanceService';
import { MaintenanceRecordDto, MaintenanceStatus, MaintenanceType } from '../../types';
import { Plus, Search, Trash2 } from 'lucide-react';
import '../vehicles/Vehicles.css';

const getMaintenanceTypeLabel = (type: MaintenanceType | number) => {
  switch (type) {
    case 0: return 'Preventive';
    case 1: return 'Corrective';
    case 2: return 'Predictive';
    case 3: return 'Routine';
    default: return 'Unknown';
  }
};

const getMaintenanceStatusLabel = (status: MaintenanceStatus | number) => {
  switch (status) {
    case 0: return 'Scheduled';
    case 1: return 'In Progress';
    case 2: return 'Completed';
    case 3: return 'Cancelled';
    default: return 'Unknown';
  }
};

const Maintenance = () => {
  const [data, setData] = useState<MaintenanceRecordDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    fetchMaintenance();
  }, []);

  const fetchMaintenance = async () => {
    try {
      setLoading(true);
      setError('');
      const records = await maintenanceService.getMaintenanceRecords();
      setData(Array.isArray(records) ? records : []);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Network error: Could not reach the server');
    } finally {
      setLoading(false);
    }
  };

  const deleteMaintenance = async (record: MaintenanceRecordDto) => {
    if (!window.confirm(`Delete maintenance record for ${record.vehicleRegistration || 'this vehicle'}?`)) return;
    try {
      await maintenanceService.deleteMaintenanceRecord(record.id);
      await fetchMaintenance();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Could not delete maintenance record.');
    }
  };

  return (
    <div className="vehicles-container fade-in">
      <div className="page-header">
        <h1>Maintenance & Servicing</h1>
        <button className="btn btn-primary" onClick={() => navigate('/maintenance/new')}>
          <Plus size={18} /> Schedule Maintenance
        </button>
      </div>

      <div className="table-controls glass-panel">
        <div className="search-box">
          <Search size={18} className="search-icon" />
          <input type="text" placeholder="Search maintenance records..." />
        </div>
      </div>

      <div className="data-table-wrapper glass-panel">
        {loading ? (
          <div className="loading-state">Loading records...</div>
        ) : error ? (
          <div className="error-state">
            <p>{error}</p>
            <button className="btn btn-primary" onClick={fetchMaintenance}>Retry</button>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Vehicle</th>
                <th>Type</th>
                <th>Scheduled Date</th>
                <th>Description</th>
                <th>Provider</th>
                <th>Cost</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.length > 0 ? (
                data.map(record => (
                  <tr key={record.id} className={record.isOverdue ? 'overdue-row' : ''}>
                    <td><strong>{record.vehicleRegistration || 'N/A'}</strong></td>
                    <td>{getMaintenanceTypeLabel(record.type)}</td>
                    <td>
                      {new Date(record.scheduledDate).toLocaleDateString()}
                      {record.isOverdue && <span style={{color: '#ff4d4f', fontSize: '0.8rem', display: 'block'}}>Overdue</span>}
                    </td>
                    <td>{record.description}</td>
                    <td>{record.serviceProvider || 'N/A'}</td>
                    <td>${(record.cost ?? 0).toFixed(2)}</td>
                    <td>
                      <span className={`status-badge status-${getMaintenanceStatusLabel(record.status).toLowerCase().replace(/\s+/g, '-')}`}>
                        {getMaintenanceStatusLabel(record.status)}
                      </span>
                    </td>
                    <td>
                      <Link className="action-btn" to={`/maintenance/${record.id}/edit`}>Edit</Link>{' '}
                      <button className="action-btn" onClick={() => deleteMaintenance(record)} title="Delete maintenance record"><Trash2 size={14} /></button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={8} className="empty-state">No maintenance records found. Click "Schedule Maintenance" to create one.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Maintenance;
