import { api } from './api';
import { ApiResponse, PagedResult, FuelRecordDto } from '../types';

export const fuelService = {
  getFuelRecords: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<FuelRecordDto>>>('/fuel', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  }
};
