import { api } from './api';
import { MaintenanceRecordDto } from '../types';

export const maintenanceService = {
  getMaintenanceRecords: async (pageNumber = 1, pageSize = 100) => {
    // Backend returns raw array (not wrapped in ApiResponse)
    const response = await api.get<MaintenanceRecordDto[] | any>('/maintenance', {
      params: { pageNumber, pageSize }
    });
    const data = response.data;
    if (Array.isArray(data)) return data as MaintenanceRecordDto[];
    if (data && typeof data === 'object' && 'data' in data && Array.isArray(data.data)) {
      return data.data as MaintenanceRecordDto[];
    }
    return (data as unknown) as MaintenanceRecordDto[];
  },

  getMaintenanceRecordById: async (id: string) => {
    const response = await api.get<MaintenanceRecordDto>(`/maintenance/${id}`);
    return response.data;
  },

  createMaintenanceRecord: async (payload: object) => {
    const response = await api.post<MaintenanceRecordDto>('/maintenance', payload);
    return response.data;
  },

  updateMaintenanceRecord: async (id: string, payload: object) => {
    const response = await api.put<MaintenanceRecordDto>(`/maintenance/${id}`, payload);
    return response.data;
  },

  deleteMaintenanceRecord: async (id: string) => api.delete(`/maintenance/${id}`)
};
