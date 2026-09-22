import { api } from './api';
import { ApiResponse, PagedResult, InspectionDto } from '../types';

export const inspectionService = {
  getInspections: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<InspectionDto>>>('/inspections', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  }
};
