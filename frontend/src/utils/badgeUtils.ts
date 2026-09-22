/**
 * getStatusBadge — Returns a badge class based on a generic status string.
 * Handles VehicleStatus, DriverStatus, TripStatus, etc.
 */
export const getStatusBadge = (status: string): string => {
  const s = (status || '').toLowerCase().replace(/\s+/g, '');
  if (['active', 'available', 'completed', 'passed', 'open'].includes(s)) return 'badge badge-success';
  if (['maintenance', 'inprogress', 'inservice', 'underreview', 'pending', 'scheduled'].includes(s)) return 'badge badge-warning';
  if (['outofservice', 'inactive', 'retired', 'failed', 'cancelled', 'closed', 'resolved', 'expired'].includes(s)) return 'badge badge-danger';
  if (['ontrip', 'started', 'critical', 'high'].includes(s)) return 'badge badge-primary';
  if (['low', 'minor', 'info'].includes(s)) return 'badge badge-info';
  return 'badge badge-default';
};
