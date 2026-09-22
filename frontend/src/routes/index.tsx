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
import Assignments from '../pages/assignments';

const AppRoutes = () => {
  // In a real app, check auth state
  const isAuthenticated = true;

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        
        {/* Protected Routes */}
        <Route path="/" element={isAuthenticated ? <MainLayout /> : <Navigate to="/login" />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="vehicles" element={<Vehicles />} />
          <Route path="drivers" element={<Drivers />} />
          <Route path="trips" element={<Trips />} />
          <Route path="maintenance" element={<Maintenance />} />
          <Route path="inspections" element={<Inspections />} />
          <Route path="incidents" element={<Incidents />} />
          <Route path="fuel" element={<Fuel />} />
          <Route path="assignments" element={<Assignments />} />
          {/* Fallback */}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
};

export default AppRoutes;
