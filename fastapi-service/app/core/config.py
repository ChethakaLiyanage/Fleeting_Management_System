import os
from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    app_name: str = "Fleet Management Analytics API"
    app_env: str = os.getenv("APP_ENV", "development")
    log_level: str = os.getenv("LOG_LEVEL", "INFO")

settings = Settings()
