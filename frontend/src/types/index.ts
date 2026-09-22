export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export enum VehicleType {
  Car,
  Truck,
  Van,
  Motorcycle,
  Bus
}

export enum FuelType {
  Gasoline,
  Diesel,
  Electric,
  Hybrid,
  Other
}

export enum TransmissionType {
  Automatic,
  Manual
}

export enum VehicleStatus {
  Active,
  InMaintenance,
  OutOfService,
  Retired
}

export interface VehicleDto {
  id: string;
  registrationNumber: string;
  vin?: string;
  engineNumber: string;
  make: string;
  model: string;
  year: number;
  vehicleType: VehicleType;
  fuelType: FuelType;
  transmission: TransmissionType;
  color: string;
  mileage: number;
  status: VehicleStatus;
  purchaseDate?: string;
  purchasePrice?: number;
  registrationExpiry?: string;
  createdAt: string;
  updatedAt?: string;
}

export enum DriverStatus {
  Active,
  OnLeave,
  Suspended,
  Terminated
}

export interface DriverDto {
  id: string;
  userId?: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phone: string;
  email?: string;
  address: string;
  licenseNumber: string;
  licenseClass: string;
  licenseIssueDate: string;
  licenseExpiry: string;
  isLicenseExpired: boolean;
  status: DriverStatus;
  joinDate: string;
  emergencyContact: string;
  createdAt: string;
  updatedAt?: string;
}

export interface TripDto {
  id: string;
  vehicleId: string;
  driverId: string;
  startLocation: string;
  endLocation: string;
  startTime: string;
  endTime?: string;
  distance?: number;
  status: string;
}

export interface MaintenanceRecordDto {
  id: string;
  vehicleId: string;
  date: string;
  description: string;
  cost: number;
  provider: string;
  status: string;
}

export interface InspectionDto {
  id: string;
  vehicleId: string;
  inspectorId?: string;
  date: string;
  status: string;
  result: string;
  notes?: string;
}

export interface IncidentDto {
  id: string;
  vehicleId: string;
  driverId?: string;
  date: string;
  description: string;
  severity: string;
  status: string;
}

export interface FuelRecordDto {
  id: string;
  vehicleId: string;
  date: string;
  volume: number;
  cost: number;
  location: string;
}

export interface VehicleAssignmentDto {
  id: string;
  vehicleId: string;
  driverId: string;
  startDate: string;
  endDate?: string;
  status: string;
}
