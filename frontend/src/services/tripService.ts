import { api } from './api';
import { ApiResponse, PagedResult, TripDto } from '../types';

export const tripService = {
  getTrips: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<TripDto>>>('/trips', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  },

  getTripById: async (id: string) => {
    const response = await api.get<ApiResponse<TripDto>>(`/trips/${id}`);
    return response.data;
  },

  createTrip: async (payload: object) => {
    const response = await api.post<ApiResponse<TripDto>>('/trips', payload);
    return response.data;
  },

  updateTrip: async (id: string, payload: object) => {
    const response = await api.put<ApiResponse<TripDto>>(`/trips/${id}`, payload);
    return response.data;
  },

  deleteTrip: async (id: string) => api.delete(`/trips/${id}`)
};
