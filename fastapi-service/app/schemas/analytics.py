from pydantic import BaseModel
from typing import List

class FuelDataPoint(BaseModel):
    vehicle_id: str
    odometer: float
    litres: float
    cost: float

class FuelAnalyticsResponse(BaseModel):
    efficiency_trend: str
    anomalies_detected: int

class MaintenancePredictRequest(BaseModel):
    vehicle_id: str
    current_odometer: float
    history_records: int

class MaintenancePredictResponse(BaseModel):
    risk_level: str
    recommendation: str

class CostForecastRequest(BaseModel):
    historical_costs: List[float]
    months_to_forecast: int

class CostForecastResponse(BaseModel):
    forecasted_costs: List[float]

class RouteOptimizeRequest(BaseModel):
    start_location: str
    stops: List[str]

class RouteOptimizeResponse(BaseModel):
    optimized_sequence: List[str]
    total_distance_km: float
