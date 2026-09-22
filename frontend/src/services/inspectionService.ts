import { api } from './api';
import { ApiResponse, PagedResult, InspectionDto } from '../types';

export const inspectionService = {
  getInspections: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<InspectionDto>>>('/inspections', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },

  getInspectionById: async (id: string) => {
    const response = await api.get<ApiResponse<InspectionDto>>(`/inspections/${id}`);
    return response.data;
  },

  createInspection: async (payload: object) => {
    const response = await api.post<ApiResponse<InspectionDto>>('/inspections', payload);
    return response.data;
  },

  updateInspection: async (id: string, payload: object) => {
    const response = await api.put<ApiResponse<InspectionDto>>(`/inspections/${id}`, payload);
    return response.data;
  },

  deleteInspection: async (id: string) => api.delete(`/inspections/${id}`)
};
