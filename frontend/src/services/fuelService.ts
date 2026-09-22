import { api } from './api';
import { ApiResponse, FuelRecordDto } from '../types';

export const fuelService = {
  getFuelRecords: async (pageNumber = 1, pageSize = 100) => {
    // Backend returns raw array (not wrapped in ApiResponse), so we handle both
    const response = await api.get<FuelRecordDto[] | ApiResponse<FuelRecordDto[]>>('/fuel', {
      params: { pageNumber, pageSize }
    });
    const data = response.data;
    // Handle both wrapped and unwrapped responses
    if (Array.isArray(data)) return data as FuelRecordDto[];
    if (data && typeof data === 'object' && 'data' in data && Array.isArray((data as any).data)) {
      return (data as any).data as FuelRecordDto[];
    }
    return (data as unknown) as FuelRecordDto[];
  },

  getFuelRecordById: async (id: string) => {
    const response = await api.get<FuelRecordDto>(`/fuel/${id}`);
    return response.data;
  },

  createFuelRecord: async (payload: object) => {
    const response = await api.post<FuelRecordDto>('/fuel', payload);
    return response.data;
  },

  updateFuelRecord: async (id: string, payload: object) => {
    const response = await api.put<FuelRecordDto>(`/fuel/${id}`, payload);
    return response.data;
  },

  deleteFuelRecord: async (id: string) => api.delete(`/fuel/${id}`)
};
