import { api } from './api';
import { ApiResponse, PagedResult, IncidentDto } from '../types';

export const incidentService = {
  getIncidents: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<IncidentDto>>>('/incidents', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  }
};
