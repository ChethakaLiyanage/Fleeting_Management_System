import { api } from './api';
import { ApiResponse, PagedResult, TripDto } from '../types';

export const tripService = {
  getTrips: async (pageNumber = 1, pageSize = 10) => {
    const response = await api.get<ApiResponse<PagedResult<TripDto>>>('/trips', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  }
};
