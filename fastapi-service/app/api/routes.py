from fastapi import APIRouter
from app.api import analytics, maintenance, optimization

api_router = APIRouter()
api_router.include_router(analytics.router, tags=["analytics"])
api_router.include_router(maintenance.router, tags=["maintenance"])
api_router.include_router(optimization.router, tags=["optimization"])
