import { Outlet, NavLink, useLocation } from 'react-router-dom';
import {
  LayoutDashboard, Car, Users, LogOut, Map,
  Wrench, FileCheck, AlertTriangle, Droplet, Bell, Search
} from 'lucide-react';
import { authService } from '../services/authService';
import './MainLayout.css';

const routeNames: Record<string, string> = {
  '/dashboard': 'Dashboard',
  '/vehicles': 'Vehicles',
  '/drivers': 'Drivers',
  '/trips': 'Trips',
  '/maintenance': 'Maintenance',
  '/inspections': 'Inspections',
  '/incidents': 'Incidents',
  '/fuel': 'Fuel Records',
};

const MainLayout = () => {
  const location = useLocation();
  const currentPage = routeNames[location.pathname] || 'Fleet OS';

  return (
    <div className="layout-container">
      {/* Dark Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <div className="logo-icon">
            <Car size={22} color="white" />
          </div>
          <h2>Fleet<span>OS</span></h2>
        </div>

        <nav className="sidebar-nav">
          <span className="nav-section-label">Overview</span>
          <NavLink to="/dashboard" className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
            <LayoutDashboard size={18} />
            <span>Dashboard</span>
          </NavLink>

          <span className="nav-section-label">Fleet</span>
          <NavLink to="/vehicles" className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
            <Car size={18} />
            <span>Vehicles</span>
          </NavLink>

          <NavLink to="/drivers" className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
            <Users size={18} />
            <span>Drivers</span>
          </NavLink>

          <NavLink to="/trips" className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
            <Map size={18} />
            <span>Trips</span>
          </NavLink>

          <span className="nav-section-label">Operations</span>
          <NavLink to="/fuel" className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
            <Droplet size={18} />
            <span>Fuel Records</span>
          </NavLink>

          <NavLink to="/maintenance" className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
            <Wrench size={18} />
            <span>Maintenance</span>
          </NavLink>

          <NavLink to="/inspections" className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
            <FileCheck size={18} />
            <span>Inspections</span>
          </NavLink>

          <NavLink to="/incidents" className={({ isActive }) => isActive ? 'nav-item active' : 'nav-item'}>
            <AlertTriangle size={18} />
            <span>Incidents</span>
          </NavLink>

          <div className="nav-divider" />

          <button className="nav-item btn-logout" onClick={() => authService.logout()}>
            <LogOut size={18} />
            <span>Logout</span>
          </button>
        </nav>
      </aside>

      {/* Main Content Area */}
      <main className="main-content">
        {/* White Topbar */}
        <header className="top-header">
          <div className="header-left">
            <div>
              <div className="page-breadcrumb">Fleet Management System</div>
              <div className="page-title-nav">{currentPage}</div>
            </div>
          </div>

          <div className="header-search">
            <div className="search-input-wrapper">
              <Search size={15} className="search-icon" />
              <input type="text" placeholder="Search anything..." className="search-input" />
            </div>
          </div>

          <div className="header-actions">
            <button className="icon-btn notif-btn">
              <Bell size={16} />
              <span className="notif-dot" />
            </button>

            <div className="user-info">
              <div className="user-avatar">A</div>
              <div>
                <div className="user-name">Admin</div>
                <div className="user-role">Administrator</div>
              </div>
            </div>
          </div>
        </header>

        <div className="page-content">
          <Outlet />
        </div>
      </main>
    </div>
  );
};

export default MainLayout;
