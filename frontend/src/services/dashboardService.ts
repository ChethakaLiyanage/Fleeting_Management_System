import { api } from './api';

export interface DashboardSummaryDto {
  totalVehicles: number;
  availableVehicles: number;
  vehiclesInUse: number;
  totalDrivers: number;
  availableDrivers: number;
  tripsToday: number;
  activeTrips: number;
  vehiclesUnderMaintenance: number;
  openIncidentsCount: number;
}

export const dashboardService = {
  getSummary: async () => {
    // Note: The DashboardController returns the raw DTO, not wrapped in ApiResponse!
    // But let's check. Actually, in DashboardController, it just returns Ok(await ...). It does not use ApiResponse<T>.Ok().
    const response = await api.get<any>('/Dashboard/summary');
    return response.data;
  },
  
  getAlerts: async () => {
    const response = await api.get<any>('/Dashboard/alerts');
    return response.data;
  }
};
