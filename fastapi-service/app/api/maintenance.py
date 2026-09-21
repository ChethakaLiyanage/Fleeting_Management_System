from fastapi import APIRouter
from app.schemas.analytics import MaintenancePredictRequest, MaintenancePredictResponse
from app.services.maintenance_prediction import predict_maintenance_risk

router = APIRouter()

@router.post("/predict-maintenance", response_model=MaintenancePredictResponse)
async def predict_maintenance(req: MaintenancePredictRequest):
    result = predict_maintenance_risk(req.vehicle_id, req.current_odometer)
    return result
