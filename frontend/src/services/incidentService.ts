import { api } from './api';
import { ApiResponse, PagedResult, IncidentDto } from '../types';

export const incidentService = {
  getIncidents: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<IncidentDto>>>('/incidents', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },

  getIncidentById: async (id: string) => {
    const response = await api.get<ApiResponse<IncidentDto>>(`/incidents/${id}`);
    return response.data;
  },

  createIncident: async (payload: object) => {
    const response = await api.post<ApiResponse<IncidentDto>>('/incidents', payload);
    return response.data;
  },

  updateIncident: async (id: string, payload: object) => {
    const response = await api.put<ApiResponse<IncidentDto>>(`/incidents/${id}`, payload);
    return response.data;
  },

  deleteIncident: async (id: string) => api.delete(`/incidents/${id}`)
};
