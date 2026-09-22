import { api } from './api';
import { ApiResponse, PagedResult, VehicleDto } from '../types';

export const vehicleService = {
  getVehicles: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<VehicleDto>>>('/vehicles', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },
  
  getVehicleById: async (id: string) => {
    const response = await api.get<ApiResponse<VehicleDto>>(`/vehicles/${id}`);
    return response.data;
  },

  createVehicle: async (payload: object) => {
    const response = await api.post<ApiResponse<VehicleDto>>('/vehicles', payload);
    return response.data;
  },

  updateVehicle: async (id: string, payload: object) => {
    const response = await api.put<ApiResponse<VehicleDto>>(`/vehicles/${id}`, payload);
    return response.data;
  },

  deleteVehicle: async (id: string) => api.delete(`/vehicles/${id}`)
};
