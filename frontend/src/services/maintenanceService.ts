import { api } from './api';
import { ApiResponse, PagedResult, MaintenanceRecordDto } from '../types';

export const maintenanceService = {
  getMaintenanceRecords: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<MaintenanceRecordDto>>>('/maintenance', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  }
};
