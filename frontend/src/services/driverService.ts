import { api } from './api';
import { ApiResponse, PagedResult, DriverDto } from '../types';

export const driverService = {
  getDrivers: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<DriverDto>>>('/drivers', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },
  
  getDriverById: async (id: string) => {
    const response = await api.get<ApiResponse<DriverDto>>(`/drivers/${id}`);
    return response.data;
  },

  createDriver: async (payload: object) => {
    const response = await api.post<ApiResponse<DriverDto>>('/drivers', payload);
    return response.data;
  },

  updateDriver: async (id: string, payload: object) => {
    const response = await api.put<ApiResponse<DriverDto>>(`/drivers/${id}`, payload);
    return response.data;
  },

  deleteDriver: async (id: string) => api.delete(`/drivers/${id}`)
};
