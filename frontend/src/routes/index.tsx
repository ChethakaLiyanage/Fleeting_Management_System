import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from '../layouts/MainLayout';
import Dashboard from '../pages/dashboard';
import Vehicles from '../pages/vehicles';
import Drivers from '../pages/drivers';
import Login from '../pages/auth/Login';
import Trips from '../pages/trips';
import Maintenance from '../pages/maintenance';
import Inspections from '../pages/inspections';
import Incidents from '../pages/incidents';
import Fuel from '../pages/fuel';
import EditRecord from '../pages/edit';

import { authService } from '../services/authService';

const ProtectedRoute = () => {
  if (!authService.isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }
  return <MainLayout />;
};

const AppRoutes = () => {
  return (
    <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
      <Routes>
        <Route path="/login" element={<Login />} />
        
        {/* Protected Routes */}
        <Route path="/" element={<ProtectedRoute />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="vehicles" element={<Vehicles />} />
          <Route path="drivers" element={<Drivers />} />
          <Route path="trips" element={<Trips />} />
          <Route path="maintenance" element={<Maintenance />} />
          <Route path="inspections" element={<Inspections />} />
          <Route path="incidents" element={<Incidents />} />
          <Route path="fuel" element={<Fuel />} />
          <Route path=":module/new" element={<EditRecord />} />
          <Route path=":module/:id/edit" element={<EditRecord />} />
          {/* Fallback */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
};

export default AppRoutes;
