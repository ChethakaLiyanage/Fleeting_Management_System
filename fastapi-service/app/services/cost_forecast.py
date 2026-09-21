def forecast_future_costs(historical_costs: list, months: int) -> list:
    # Placeholder logic
    if not historical_costs:
        return [0.0] * months
    avg = sum(historical_costs) / len(historical_costs)
    return [avg] * months
