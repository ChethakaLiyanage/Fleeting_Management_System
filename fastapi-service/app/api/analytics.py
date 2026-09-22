from fastapi import APIRouter
from app.schemas.analytics import FuelDataPoint, FuelAnalyticsResponse
from app.services.fuel_analysis import analyze_fuel_efficiency

router = APIRouter()

@router.post("/fuel", response_model=FuelAnalyticsResponse)
async def analyze_fuel(data: list[FuelDataPoint]):
    result = analyze_fuel_efficiency(data)
    return result
