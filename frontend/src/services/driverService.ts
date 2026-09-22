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
  }
};
