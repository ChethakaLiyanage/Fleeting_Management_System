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
  }
};
