import { FormEvent, useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Save } from 'lucide-react';
import { driverService } from '../../services/driverService';
import { vehicleService } from '../../services/vehicleService';
import { tripService } from '../../services/tripService';
import { fuelService } from '../../services/fuelService';
import { maintenanceService } from '../../services/maintenanceService';
import { inspectionService } from '../../services/inspectionService';
import { incidentService } from '../../services/incidentService';
import { DriverDto, TripDto, VehicleDto } from '../../types';
import './Edit.css';

type Module = 'drivers' | 'vehicles' | 'trips' | 'fuel' | 'maintenance' | 'inspections' | 'incidents';
type FormState = Record<string, any>;

// Updated options to exactly match the backend Domain Enums
const options = {
  vehicleType: [['1', 'Sedan'], ['2', 'SUV'], ['3', 'Truck'], ['4', 'Van'], ['5', 'Bus'], ['6', 'Motorcycle'], ['7', 'Other']],
  fuelType: [['1', 'Petrol'], ['2', 'Diesel'], ['3', 'Electric'], ['4', 'Hybrid'], ['5', 'CNG']],
  vehicleStatus: [['1', 'Available'], ['2', 'Assigned'], ['3', 'On trip'], ['4', 'Maintenance'], ['5', 'Out of service'], ['6', 'Retired']],
  driverStatus: [['1', 'Available'], ['2', 'Assigned'], ['3', 'On trip'], ['4', 'On leave'], ['5', 'Suspended'], ['6', 'Inactive']],
  tripStatus: [['1', 'Scheduled'], ['2', 'Assigned'], ['3', 'In progress'], ['4', 'Completed'], ['5', 'Cancelled']],
  maintenanceType: [['0', 'Preventive'], ['1', 'Corrective'], ['2', 'Predictive maintenance'], ['3', 'Routine service']],
  maintenanceStatus: [['0', 'Scheduled'], ['1', 'In progress'], ['2', 'Completed'], ['3', 'Cancelled']],
  inspectionType: [['1', 'Pre-trip'], ['2', 'Post-trip'], ['3', 'Scheduled']],
  inspectionResult: [['1', 'Passed'], ['2', 'Failed'], ['3', 'Needs attention']],
  itemStatus: [['1', 'Pass'], ['2', 'Fail'], ['3', 'Attention']],
  incidentType: [['1', 'Accident'], ['2', 'Breakdown'], ['3', 'Damage'], ['4', 'Theft'], ['5', 'Traffic violation'], ['6', 'Other']],
  severity: [['1', 'Low'], ['2', 'Medium'], ['3', 'High'], ['4', 'Critical']],
  incidentStatus: [['1', 'Reported'], ['2', 'Under investigation'], ['3', 'Repair pending'], ['4', 'Resolved'], ['5', 'Closed']],
} as Record<string, string[][]>;

const dateValue = (value?: string) => value ? new Date(value).toISOString().slice(0, 10) : '';
const dateTimeValue = (value?: string) => value ? new Date(value).toISOString().slice(0, 16) : '';
const asNumber = (value: any) => value === '' || value === null || value === undefined ? null : Number(value);
const enumValue = (value: any, labels: string[][]) => {
  if (typeof value === 'number') return String(value);
  const match = labels.find(([, label]) => label.toLowerCase() === String(value).toLowerCase());
  return match?.[0] ?? '';
};

function Field({ label, name, value, onChange, type = 'text', required = false, visible = true, options: selectOptions, fullWidth = false }: { label: string; name: string; value: any; onChange: (name: string, value: string) => void; type?: string; required?: boolean; visible?: boolean; options?: string[][]; fullWidth?: boolean }) {
  if (!visible) return null;
  return (
    <div className={`form-group ${fullWidth ? 'full-width' : ''}`}>
      <label>{label}</label>
      {selectOptions ? (
        <select className="form-control" name={name} value={value ?? ''} onChange={event => onChange(name, event.target.value)} required={required}>
          <option value="">Select {label.toLowerCase()}</option>
          {selectOptions.map(([optionValue, optionLabel]) => <option key={optionValue} value={optionValue}>{optionLabel}</option>)}
        </select>
      ) : type === 'textarea' ? (
        <textarea className="form-control" name={name} value={value ?? ''} onChange={event => onChange(name, event.target.value)} required={required} />
      ) : (
        <input className="form-control" name={name} type={type} value={value ?? ''} onChange={event => onChange(name, event.target.value)} required={required} />
      )}
    </div>
  );
}

const EditRecord = () => {
  const { module, id } = useParams<{ module: Module; id: string }>();
  const navigate = useNavigate();
  const [form, setForm] = useState<FormState>({});
  const [vehicles, setVehicles] = useState<VehicleDto[]>([]);
  const [drivers, setDrivers] = useState<DriverDto[]>([]);
  const [trips, setTrips] = useState<TripDto[]>([]);
  const [loading, setLoading] = useState(Boolean(id));
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');

  const update = (name: string, value: string) => setForm(previous => ({ ...previous, [name]: value }));
  let title = 'Record';
  if (module) {
    const isEdit = !!id;
    if (module === 'fuel') title = isEdit ? 'Edit Fuel Record' : 'Add Fuel Record';
    else if (module === 'drivers') title = isEdit ? 'Edit Driver' : 'Add New Driver';
    else if (module === 'vehicles') title = isEdit ? 'Edit Vehicle' : 'Add Vehicle';
    else if (module === 'trips') title = isEdit ? 'Edit Trip' : 'Log New Trip';
    else if (module === 'incidents') title = isEdit ? 'Edit Incident' : 'Report Incident';
    else if (module === 'maintenance') title = isEdit ? 'Edit Maintenance' : 'Schedule Maintenance';
    else if (module === 'inspections') title = isEdit ? 'Edit Inspection' : 'New Inspection';
    else title = `${isEdit ? 'Edit' : 'Add'} ${(module as string).charAt(0).toUpperCase() + (module as string).slice(1)}`;
  }

  useEffect(() => {
    if (!module) return;
    const load = async () => {
      try {
        setLoading(true);
        const related = await Promise.all([vehicleService.getVehicles(1, 1000), driverService.getDrivers(1, 1000), tripService.getTrips(1, 1000)]);
        setVehicles(related[0].data?.items ?? []);
        setDrivers(related[1].data?.items ?? []);
        setTrips(related[2].data?.items ?? []);
        if (!id) {
          setForm(defaultForm(module));
          return;
        }
        let record: any;
        if (module === 'vehicles') record = (await vehicleService.getVehicleById(id)).data;
        if (module === 'drivers') record = (await driverService.getDriverById(id)).data;
        if (module === 'trips') record = (await tripService.getTripById(id)).data;
        if (module === 'fuel') record = await fuelService.getFuelRecordById(id);
        if (module === 'maintenance') record = await maintenanceService.getMaintenanceRecordById(id);
        if (module === 'inspections') record = (await inspectionService.getInspectionById(id)).data;
        if (module === 'incidents') record = (await incidentService.getIncidentById(id)).data;
        if (!record) throw new Error('Record was not found.');
        setForm(toForm(module, record));
      } catch (loadError: any) {
        setError(loadError.response?.data?.message || loadError.message || 'Could not load this record.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [module, id]);

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    if (!module) return;
    try {
      setSaving(true);
      setError('');
      if (id) await updateRecord(module, id, form);
      else await createRecord(module, form);
      setMessage('Changes saved successfully.');
      setTimeout(() => navigate(`/${module}`), 500);
    } catch (saveError: any) {
      setError(saveError.response?.data?.message || saveError.message || 'Could not save changes.');
    } finally {
      setSaving(false);
    }
  };

  return <div className="edit-container fade-in">
    <div className="page-header" style={{ marginBottom: '1.5rem', padding: '0 1rem' }}>
      <div className="page-header-left">
        <Link className="back-link" to={`/${module}`} style={{ display: 'inline-flex', alignItems: 'center', gap: '0.25rem', fontSize: '0.875rem', color: 'var(--text-muted)', marginBottom: '0.5rem', textDecoration: 'none' }}>
          <ArrowLeft size={14} /> Back to {module}
        </Link>
        <h1 style={{ fontSize: '1.5rem', fontWeight: 700, margin: 0 }}>{title}</h1>
      </div>
    </div>
    
    {loading ? <div className="loading-state" style={{ padding: '3rem', textAlign: 'center' }}>Loading record...</div> : error && !Object.keys(form).length ? <div className="state-container"><p>{error}</p><Link className="btn btn-primary" to={`/${module}`}>Back to list</Link></div> : 
    <div className="form-card">
      <form onSubmit={submit}>
        {error && <div className="form-error">{error}</div>}
        {message && <div className="form-success">{message}</div>}
        <div className="form-grid">{module && renderFields(module, form, update, vehicles, drivers, trips, !id)}</div>
        <div className="form-actions">
          <Link className="btn btn-secondary" to={`/${module}`}>Cancel</Link>
          <button className="btn btn-primary" type="submit" disabled={saving}>
            <Save size={16} /> {saving ? 'Saving...' : id ? 'Save changes' : 'Create record'}
          </button>
        </div>
      </form>
    </div>}
  </div>;
};

function renderFields(module: Module, form: FormState, update: (name: string, value: string) => void, vehicles: VehicleDto[], drivers: DriverDto[], trips: TripDto[], isCreate: boolean) {
  const vehicleOptions = vehicles.map(vehicle => [vehicle.id, `${vehicle.registrationNumber} - ${vehicle.make} ${vehicle.model}`]);
  const driverOptions = drivers.map(driver => [driver.id, `${driver.employeeNumber} - ${driver.fullName}`]);
  const tripOptions = trips.map(trip => [trip.id, `${trip.tripNumber} - ${trip.startLocation} to ${trip.destination}`]);
  if (module === 'vehicles') return <>
    <div className="form-section-title">Vehicle Details</div>
    <Field label="Registration Number" name="registrationNumber" value={form.registrationNumber} onChange={update} required />
    <Field label="VIN" name="vin" value={form.vin} onChange={update} />
    <Field label="Engine Number" name="engineNumber" value={form.engineNumber} onChange={update} required />
    <Field label="Make" name="make" value={form.make} onChange={update} required />
    <Field label="Model" name="model" value={form.model} onChange={update} required />
    <Field label="Year" name="year" value={form.year} onChange={update} type="number" required />
    <Field label="Vehicle Type" name="vehicleType" value={form.vehicleType} onChange={update} options={options.vehicleType} required />
    <Field label="Fuel Type" name="fuelType" value={form.fuelType} onChange={update} options={options.fuelType} required />
    <Field label="Transmission" name="transmission" value={form.transmission} onChange={update} options={[['1', 'Automatic'], ['2', 'Manual']]} required />
    <Field label="Color" name="color" value={form.color} onChange={update} required />
    <Field label="Mileage" name="mileage" value={form.mileage} onChange={update} type="number" required />
    <Field label="Status" name="status" value={form.status} onChange={update} options={options.vehicleStatus} required visible={!isCreate} />
    <div className="form-section-title">Purchase Details</div>
    <Field label="Purchase Date" name="purchaseDate" value={form.purchaseDate} onChange={update} type="date" />
    <Field label="Purchase Price" name="purchasePrice" value={form.purchasePrice} onChange={update} type="number" />
    <Field label="Registration Expiry" name="registrationExpiry" value={form.registrationExpiry} onChange={update} type="date" />
  </>;
  if (module === 'drivers') return <>
    <div className="form-section-title">Personal Information</div>
    <Field label="Employee Number" name="employeeNumber" value={form.employeeNumber} onChange={update} required />
    <Field label="First Name" name="firstName" value={form.firstName} onChange={update} required />
    <Field label="Last Name" name="lastName" value={form.lastName} onChange={update} required />
    <Field label="Phone" name="phone" value={form.phone} onChange={update} required />
    <Field label="Email" name="email" value={form.email} onChange={update} type="email" required />
    <Field label="Initial Password" name="initialPassword" value={form.initialPassword} onChange={update} type="password" required={isCreate} visible={isCreate} />
    <div className="form-section-title">Address</div>
    <Field label="Address" name="address" value={form.address} onChange={update} required fullWidth />
    <div className="form-section-title">Licence Information</div>
    <Field label="License Number" name="licenseNumber" value={form.licenseNumber} onChange={update} required />
    <Field label="License Class" name="licenseClass" value={form.licenseClass} onChange={update} required />
    <Field label="License Issue Date" name="licenseIssueDate" value={form.licenseIssueDate} onChange={update} type="date" required />
    <Field label="License Expiry" name="licenseExpiry" value={form.licenseExpiry} onChange={update} type="date" required />
    <div className="form-section-title">Employment</div>
    <Field label="Join Date" name="joinDate" value={form.joinDate} onChange={update} type="date" visible={isCreate} />
    <Field label="Emergency Contact" name="emergencyContact" value={form.emergencyContact} onChange={update} required />
    <Field label="Status" name="status" value={form.status} onChange={update} options={options.driverStatus} required visible={!isCreate} />
  </>;
  if (module === 'trips') return <>
    <div className="form-section-title">Trip Details</div>
    <Field label="Trip Number" name="tripNumber" value={form.tripNumber} onChange={update} required visible={!isCreate} />
    <Field label="Status" name="status" value={form.status} onChange={update} options={options.tripStatus} required visible={!isCreate} />
    <Field label="Vehicle" name="vehicleId" value={form.vehicleId} onChange={update} options={vehicleOptions} required />
    <Field label="Driver" name="driverId" value={form.driverId} onChange={update} options={driverOptions} required />
    <Field label="Start Location" name="startLocation" value={form.startLocation} onChange={update} required />
    <Field label="Destination" name="destination" value={form.destination} onChange={update} required />
    <Field label="Start Time" name="startTime" value={form.startTime} onChange={update} type="datetime-local" />
    <Field label="End Time" name="endTime" value={form.endTime} onChange={update} type="datetime-local" />
    <Field label="Starting Mileage" name="startingMileage" value={form.startingMileage} onChange={update} type="number" />
    <Field label="Ending Mileage" name="endingMileage" value={form.endingMileage} onChange={update} type="number" />
    <Field label="Distance" name="distance" value={form.distance} onChange={update} type="number" />
    <Field label="Purpose" name="purpose" value={form.purpose} onChange={update} required fullWidth />
    <Field label="Notes" name="notes" value={form.notes} onChange={update} type="textarea" fullWidth />
  </>;
  if (module === 'fuel') return <>
    <div className="form-section-title">Fuel Record Details</div>
    <Field label="Vehicle" name="vehicleId" value={form.vehicleId} onChange={update} options={vehicleOptions} required />
    <Field label="Driver" name="driverId" value={form.driverId} onChange={update} options={driverOptions} />
    <Field label="Fuel Date" name="fuelDate" value={form.fuelDate} onChange={update} type="datetime-local" required />
    <Field label="Fuel Type" name="fuelType" value={form.fuelType} onChange={update} options={options.fuelType} required />
    <Field label="Litres" name="litres" value={form.litres} onChange={update} type="number" required />
    <Field label="Cost Per Litre" name="costPerLitre" value={form.costPerLitre} onChange={update} type="number" required />
    <Field label="Odometer Reading" name="odometerReading" value={form.odometerReading} onChange={update} type="number" required />
    <Field label="Station" name="station" value={form.station} onChange={update} />
    <Field label="Notes" name="notes" value={form.notes} onChange={update} type="textarea" fullWidth />
  </>;
  if (module === 'maintenance') return <>
    <div className="form-section-title">Maintenance Record Details</div>
    <Field label="Vehicle" name="vehicleId" value={form.vehicleId} onChange={update} options={vehicleOptions} required />
    <Field label="Type" name="type" value={form.type} onChange={update} options={options.maintenanceType} required />
    <Field label="Status" name="status" value={form.status} onChange={update} options={options.maintenanceStatus} required visible={!isCreate} />
    <Field label="Service Provider" name="serviceProvider" value={form.serviceProvider} onChange={update} />
    <Field label="Scheduled Date" name="scheduledDate" value={form.scheduledDate} onChange={update} type="datetime-local" required />
    <Field label="Completed Date" name="completedDate" value={form.completedDate} onChange={update} type="datetime-local" />
    <Field label="Odometer Reading" name="odometerReading" value={form.odometerReading} onChange={update} type="number" />
    <Field label="Cost" name="cost" value={form.cost} onChange={update} type="number" />
    <Field label="Next Service Odometer" name="nextServiceOdometer" value={form.nextServiceOdometer} onChange={update} type="number" />
    <Field label="Next Service Date" name="nextServiceDate" value={form.nextServiceDate} onChange={update} type="datetime-local" />
    <Field label="Description" name="description" value={form.description} onChange={update} type="textarea" required fullWidth />
    <Field label="Notes" name="notes" value={form.notes} onChange={update} type="textarea" fullWidth />
  </>;
  if (module === 'inspections') return <>
    <div className="form-section-title">Inspection Details</div>
    <Field label="Vehicle" name="vehicleId" value={form.vehicleId} onChange={update} options={vehicleOptions} required />
    <Field label="Driver" name="driverId" value={form.driverId} onChange={update} options={driverOptions} />
    <Field label="Trip" name="tripId" value={form.tripId} onChange={update} options={tripOptions} />
    <Field label="Type" name="type" value={form.type} onChange={update} options={options.inspectionType} required />
    <Field label="Inspection Date" name="inspectionDate" value={form.inspectionDate} onChange={update} type="datetime-local" required />
    <Field label="Result" name="result" value={form.result} onChange={update} options={options.inspectionResult} required visible={!isCreate} />
    <Field label="Notes" name="notes" value={form.notes} onChange={update} type="textarea" fullWidth />
    <div className="form-section-title" style={{ marginTop: '16px' }}>Inspection Items</div>
    <div className="form-group full-width">
      {(form.items ?? []).map((item: any, index: number) => (
        <div key={item.id ?? index} style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 2fr', gap: '12px', marginBottom: '12px' }}>
          <input className="form-control" value={item.itemName} placeholder="Item name" onChange={e => { const items = [...form.items]; items[index] = { ...items[index], itemName: e.target.value }; update('items', items as any); }} />
          <select className="form-control" value={item.status} onChange={e => { const items = [...form.items]; items[index] = { ...items[index], status: Number(e.target.value) }; update('items', items as any); }}>
            {options.itemStatus.map(([value, label]) => <option key={value} value={value}>{label}</option>)}
          </select>
          <input className="form-control" value={item.notes ?? ''} placeholder="Notes" onChange={e => { const items = [...form.items]; items[index] = { ...items[index], notes: e.target.value }; update('items', items as any); }} />
        </div>
      ))}
      <button type="button" className="btn btn-secondary" style={{ width: 'fit-content' }} onClick={() => update('items', [...(form.items ?? []), { itemName: '', status: 1, notes: '' }] as any)}>Add item</button>
    </div>
  </>;
  return <>
    <div className="form-section-title">Incident Details</div>
    <Field label="Vehicle" name="vehicleId" value={form.vehicleId} onChange={update} options={vehicleOptions} required />
    <Field label="Driver" name="driverId" value={form.driverId} onChange={update} options={driverOptions} />
    <Field label="Trip" name="tripId" value={form.tripId} onChange={update} options={tripOptions} />
    <Field label="Date" name="date" value={form.date} onChange={update} type="datetime-local" required />
    <Field label="Location" name="location" value={form.location} onChange={update} required />
    <Field label="Type" name="type" value={form.type} onChange={update} options={options.incidentType} required />
    <Field label="Severity" name="severity" value={form.severity} onChange={update} options={options.severity} required />
    <Field label="Status" name="status" value={form.status} onChange={update} options={options.incidentStatus} required visible={!isCreate} />
    <Field label="Police Report Number" name="policeReportNumber" value={form.policeReportNumber} onChange={update} />
    <Field label="Insurance Claim Number" name="insuranceClaimNumber" value={form.insuranceClaimNumber} onChange={update} />
    <Field label="Estimated Damage" name="estimatedDamage" value={form.estimatedDamage} onChange={update} type="number" />
    <Field label="Actual Repair Cost" name="actualRepairCost" value={form.actualRepairCost} onChange={update} type="number" />
    <Field label="Description" name="description" value={form.description} onChange={update} type="textarea" required fullWidth />
  </>;
}

function defaultForm(module: Module): FormState {
  if (module === 'inspections') return { inspectionDate: dateTimeValue(new Date().toISOString()), items: [{ itemName: '', status: '1', notes: '' }] };
  if (module === 'fuel') return { fuelDate: dateTimeValue(new Date().toISOString()) };
  if (module === 'maintenance') return { scheduledDate: dateTimeValue(new Date().toISOString()) };
  if (module === 'incidents') return { date: dateTimeValue(new Date().toISOString()) };
  if (module === 'drivers') return { joinDate: dateValue(new Date().toISOString()) };
  return {};
}

function toForm(module: Module, record: any): FormState {
  const form = { ...record };
  if (module === 'vehicles') return { ...form, vehicleType: String(form.vehicleType), fuelType: String(form.fuelType), transmission: String(form.transmission), status: String(form.status), purchaseDate: dateValue(form.purchaseDate), registrationExpiry: dateValue(form.registrationExpiry), vin: form.vin ?? '' };
  if (module === 'drivers') return { ...form, status: String(form.status), licenseIssueDate: dateValue(form.licenseIssueDate), licenseExpiry: dateValue(form.licenseExpiry), joinDate: dateValue(form.joinDate) };
  if (module === 'trips') return { ...form, status: enumValue(form.status, options.tripStatus), startTime: dateTimeValue(form.startTime), endTime: dateTimeValue(form.endTime) };
  if (module === 'fuel') return { ...form, driverId: form.driverId ?? '', fuelType: enumValue(form.fuelType, options.fuelType), fuelDate: dateTimeValue(form.fuelDate) };
  if (module === 'maintenance') return { ...form, type: String(form.type), status: String(form.status), scheduledDate: dateTimeValue(form.scheduledDate), completedDate: dateTimeValue(form.completedDate), nextServiceDate: dateTimeValue(form.nextServiceDate) };
  if (module === 'inspections') return { ...form, driverId: form.driverId ?? '', tripId: form.tripId ?? '', type: enumValue(form.type, options.inspectionType), result: enumValue(form.result, options.inspectionResult), inspectionDate: dateTimeValue(form.inspectionDate), items: (form.items ?? []).map((item: any) => ({ ...item, status: String(item.status) })) };
  return { ...form, driverId: form.driverId ?? '', tripId: form.tripId ?? '', type: String(form.type), severity: String(form.severity), status: String(form.status), date: dateTimeValue(form.date) };
}

async function updateRecord(module: Module, id: string, form: FormState) {
  if (module === 'vehicles') return vehicleService.updateVehicle(id, { registrationNumber: form.registrationNumber, vin: form.vin || null, engineNumber: form.engineNumber, make: form.make, model: form.model, year: Number(form.year), vehicleType: Number(form.vehicleType), fuelType: Number(form.fuelType), transmission: Number(form.transmission), color: form.color, mileage: Number(form.mileage), status: Number(form.status), purchaseDate: form.purchaseDate || null, purchasePrice: asNumber(form.purchasePrice), registrationExpiry: form.registrationExpiry || null });
  if (module === 'drivers') return driverService.updateDriver(id, { employeeNumber: form.employeeNumber, firstName: form.firstName, lastName: form.lastName, phone: form.phone, email: form.email, address: form.address, licenseNumber: form.licenseNumber, licenseClass: form.licenseClass, licenseIssueDate: form.licenseIssueDate, licenseExpiry: form.licenseExpiry, status: Number(form.status), emergencyContact: form.emergencyContact });
  if (module === 'trips') return tripService.updateTrip(id, { tripNumber: form.tripNumber, vehicleId: form.vehicleId, driverId: form.driverId, startLocation: form.startLocation, destination: form.destination, startTime: form.startTime || null, endTime: form.endTime || null, startingMileage: asNumber(form.startingMileage), endingMileage: asNumber(form.endingMileage), distance: asNumber(form.distance), purpose: form.purpose, status: Number(form.status), notes: form.notes || null });
  if (module === 'fuel') return fuelService.updateFuelRecord(id, { vehicleId: form.vehicleId, driverId: form.driverId || null, fuelDate: form.fuelDate, litres: Number(form.litres), costPerLitre: Number(form.costPerLitre), odometerReading: Number(form.odometerReading), fuelType: Number(form.fuelType), station: form.station || null, notes: form.notes || null });
  if (module === 'maintenance') return maintenanceService.updateMaintenanceRecord(id, { vehicleId: form.vehicleId, type: Number(form.type), description: form.description, serviceProvider: form.serviceProvider || null, scheduledDate: form.scheduledDate, completedDate: form.completedDate || null, odometerReading: asNumber(form.odometerReading), cost: asNumber(form.cost), nextServiceOdometer: asNumber(form.nextServiceOdometer), nextServiceDate: form.nextServiceDate || null, status: Number(form.status), notes: form.notes || null });
  if (module === 'inspections') return inspectionService.updateInspection(id, { vehicleId: form.vehicleId, driverId: form.driverId || null, tripId: form.tripId || null, type: Number(form.type), inspectionDate: form.inspectionDate, result: Number(form.result), notes: form.notes || null, items: (form.items ?? []).map((item: any) => ({ id: item.id || null, itemName: item.itemName, status: Number(item.status), notes: item.notes || null })) });
  return incidentService.updateIncident(id, { date: form.date, location: form.location, type: Number(form.type), description: form.description, severity: Number(form.severity), policeReportNumber: form.policeReportNumber || null, insuranceClaimNumber: form.insuranceClaimNumber || null, estimatedDamage: asNumber(form.estimatedDamage), actualRepairCost: asNumber(form.actualRepairCost), status: Number(form.status) });
}

async function createRecord(module: Module, form: FormState) {
  if (module === 'vehicles') return vehicleService.createVehicle({ registrationNumber: form.registrationNumber, vin: form.vin || null, engineNumber: form.engineNumber, make: form.make, model: form.model, year: Number(form.year), vehicleType: Number(form.vehicleType), fuelType: Number(form.fuelType), transmission: Number(form.transmission), color: form.color, mileage: Number(form.mileage), purchaseDate: form.purchaseDate || null, purchasePrice: asNumber(form.purchasePrice), registrationExpiry: form.registrationExpiry || null });
  if (module === 'drivers') return driverService.createDriver({ employeeNumber: form.employeeNumber, initialPassword: form.initialPassword, firstName: form.firstName, lastName: form.lastName, phone: form.phone, email: form.email, address: form.address, licenseNumber: form.licenseNumber, licenseClass: form.licenseClass, licenseIssueDate: form.licenseIssueDate, licenseExpiry: form.licenseExpiry, joinDate: form.joinDate || null, emergencyContact: form.emergencyContact });
  if (module === 'trips') return tripService.createTrip({ vehicleId: form.vehicleId, driverId: form.driverId, startLocation: form.startLocation, destination: form.destination, scheduledStartTime: form.startTime || null, purpose: form.purpose, notes: form.notes || null });
  if (module === 'fuel') return fuelService.createFuelRecord({ vehicleId: form.vehicleId, driverId: form.driverId || null, fuelDate: form.fuelDate, litres: Number(form.litres), costPerLitre: Number(form.costPerLitre), odometerReading: Number(form.odometerReading), fuelType: Number(form.fuelType), station: form.station || null, notes: form.notes || null });
  if (module === 'maintenance') return maintenanceService.createMaintenanceRecord({ vehicleId: form.vehicleId, type: Number(form.type), description: form.description, serviceProvider: form.serviceProvider || null, scheduledDate: form.scheduledDate, odometerReading: asNumber(form.odometerReading), cost: asNumber(form.cost), nextServiceOdometer: asNumber(form.nextServiceOdometer), nextServiceDate: form.nextServiceDate || null, notes: form.notes || null });
  if (module === 'inspections') return inspectionService.createInspection({ vehicleId: form.vehicleId, driverId: form.driverId || null, tripId: form.tripId || null, type: Number(form.type), inspectionDate: form.inspectionDate, notes: form.notes || null, items: (form.items ?? []).filter((item: any) => item.itemName?.trim()).map((item: any) => ({ itemName: item.itemName, status: Number(item.status), notes: item.notes || null })) });
  return incidentService.createIncident({ vehicleId: form.vehicleId, driverId: form.driverId || null, tripId: form.tripId || null, date: form.date || null, location: form.location, type: Number(form.type), description: form.description, severity: Number(form.severity), policeReportNumber: form.policeReportNumber || null, insuranceClaimNumber: form.insuranceClaimNumber || null, estimatedDamage: asNumber(form.estimatedDamage), actualRepairCost: asNumber(form.actualRepairCost) });
}

export default EditRecord;
