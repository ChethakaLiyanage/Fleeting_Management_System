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

// These match the backend Domain enums exactly
export enum VehicleType {
  Sedan = 1,
  SUV = 2,
  Truck = 3,
  Van = 4,
  Bus = 5,
  Motorcycle = 6,
  Other = 7,
}

export enum FuelType {
  Petrol = 1,
  Diesel = 2,
  Electric = 3,
  Hybrid = 4,
  CNG = 5,
}

export enum TransmissionType {
  Automatic = 1,
  Manual = 2,
}

export enum VehicleStatus {
  Available = 1,
  Assigned = 2,
  OnTrip = 3,
  Maintenance = 4,
  OutOfService = 5,
  Retired = 6,
}

export enum DriverStatus {
  Available = 1,
  Assigned = 2,
  OnTrip = 3,
  Leave = 4,
  Suspended = 5,
  Inactive = 6,
}

export enum TripStatus {
  Scheduled = 1,
  Assigned = 2,
  InProgress = 3,
  Completed = 4,
  Cancelled = 5,
}

export enum MaintenanceType {
  Preventive = 0,
  Corrective = 1,
  PredictiveMaintenance = 2,
  RoutineService = 3,
}

export enum MaintenanceStatus {
  Scheduled = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3,
}

export enum InspectionType {
  PreTrip = 1,
  PostTrip = 2,
  Scheduled = 3,
}

export enum InspectionResult {
  Passed = 1,
  Failed = 2,
  NeedsAttention = 3,
}

export enum InspectionItemStatus {
  Pass = 1,
  Fail = 2,
  Attention = 3,
}

export enum IncidentType {
  Accident = 1,
  Breakdown = 2,
  Damage = 3,
  Theft = 4,
  TrafficViolation = 5,
  Other = 6,
}

export enum IncidentSeverity {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4,
}

export enum IncidentStatus {
  Reported = 1,
  UnderInvestigation = 2,
  RepairPending = 3,
  Resolved = 4,
  Closed = 5,
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

export interface DriverDto {
  id: string;
  driverId: string;
  userId: string;
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
  tripNumber: string;
  vehicleId: string;
  driverId: string;
  vehicleRegistrationNumber: string;
  vehicleMakeModel: string;
  driverName: string;
  startLocation: string;
  destination: string;
  startTime?: string;
  endTime?: string;
  startingMileage?: number;
  endingMileage?: number;
  distance?: number;
  purpose: string;
  status: string;
  notes?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface MaintenanceRecordDto {
  id: string;
  vehicleId: string;
  vehicleRegistration?: string;
  type: MaintenanceType;
  scheduledDate: string;
  completedDate?: string;
  description: string;
  serviceProvider?: string;
  cost?: number;
  status: MaintenanceStatus;
  isOverdue: boolean;
  isCompleted: boolean;
  odometerReading?: number;
  nextServiceOdometer?: number;
  nextServiceDate?: string;
  notes?: string;
  createdAt?: string;
}

export interface InspectionDto {
  id: string;
  vehicleId: string;
  vehicleRegistrationNumber: string;
  vehicleMakeModel: string;
  driverId?: string;
  driverName?: string;
  tripId?: string;
  tripNumber?: string;
  type: string;
  inspectionDate: string;
  result: string;
  notes?: string;
  items?: InspectionItemDto[];
  createdAt: string;
}

export interface InspectionItemDto {
  id: string;
  itemName: string;
  status: InspectionItemStatus;
  notes?: string;
}

export interface IncidentDto {
  id: string;
  vehicleId: string;
  vehicleRegistration: string;
  driverId?: string;
  driverName?: string;
  tripId?: string;
  tripNumber?: string;
  date: string;
  location?: string;
  type?: IncidentType;
  description: string;
  severity: IncidentSeverity;
  policeReportNumber?: string;
  insuranceClaimNumber?: string;
  estimatedDamage?: number;
  actualRepairCost?: number;
  status: IncidentStatus;
}

export interface FuelRecordDto {
  id: string;
  vehicleId: string;
  vehicleRegistration?: string;
  driverId?: string;
  driverName?: string;
  fuelDate: string;
  litres: number;
  costPerLitre: number;
  totalCost: number;
  odometerReading: number;
  fuelType: string;
  station?: string;
  notes?: string;
}

// Create DTOs for frontend use
export interface CreateDriverPayload {
  initialPassword: string;
  employeeNumber: string;
  firstName: string;
  lastName: string;
  phone: string;
  email: string;
  address: string;
  licenseNumber: string;
  licenseClass: string;
  licenseIssueDate: string;
  licenseExpiry: string;
  joinDate?: string;
  emergencyContact: string;
}

export interface CreateVehiclePayload {
  registrationNumber: string;
  vin?: string | null;
  engineNumber: string;
  make: string;
  model: string;
  year: number;
  vehicleType: number;
  fuelType: number;
  transmission: number;
  color: string;
  mileage: number;
  purchaseDate?: string | null;
  purchasePrice?: number | null;
  registrationExpiry?: string | null;
}

export interface CreateTripPayload {
  vehicleId: string;
  driverId: string;
  startLocation: string;
  destination: string;
  scheduledStartTime?: string | null;
  purpose: string;
  notes?: string | null;
}

export interface CreateFuelRecordPayload {
  vehicleId: string;
  driverId?: string | null;
  fuelDate: string;
  litres: number;
  costPerLitre: number;
  odometerReading: number;
  fuelType: number;
  station?: string | null;
  notes?: string | null;
}

export interface CreateMaintenancePayload {
  vehicleId: string;
  type: number;
  description: string;
  serviceProvider?: string | null;
  scheduledDate: string;
  odometerReading?: number | null;
  cost?: number | null;
  nextServiceOdometer?: number | null;
  nextServiceDate?: string | null;
  notes?: string | null;
}

export interface CreateInspectionPayload {
  vehicleId: string;
  driverId?: string | null;
  tripId?: string | null;
  type: number;
  inspectionDate?: string | null;
  notes?: string | null;
  items: { itemName: string; status: number; notes?: string | null }[];
}

export interface CreateIncidentPayload {
  vehicleId: string;
  driverId?: string | null;
  tripId?: string | null;
  date?: string | null;
  location: string;
  type: number;
  description: string;
  severity: number;
  policeReportNumber?: string | null;
  insuranceClaimNumber?: string | null;
  estimatedDamage?: number | null;
  actualRepairCost?: number | null;
}
