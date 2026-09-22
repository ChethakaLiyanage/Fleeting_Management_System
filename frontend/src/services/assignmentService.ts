import { api } from './api';
import { ApiResponse, PagedResult, VehicleAssignmentDto } from '../types';

export const assignmentService = {
  getAssignments: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<VehicleAssignmentDto>>>('/assignments', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  }
};
