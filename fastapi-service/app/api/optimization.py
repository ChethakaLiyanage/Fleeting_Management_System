from fastapi import APIRouter
from app.schemas.analytics import CostForecastRequest, CostForecastResponse, RouteOptimizeRequest, RouteOptimizeResponse
from app.services.cost_forecast import forecast_future_costs
from app.services.route_optimizer import optimize_route

router = APIRouter()

@router.post("/forecast-cost", response_model=CostForecastResponse)
async def forecast_cost(req: CostForecastRequest):
    result = forecast_future_costs(req.historical_costs, req.months_to_forecast)
    return {"forecasted_costs": result}

@router.post("/optimize-route", response_model=RouteOptimizeResponse)
async def optimize_route_endpoint(req: RouteOptimizeRequest):
    result = optimize_route(req.start_location, req.stops)
    return result
