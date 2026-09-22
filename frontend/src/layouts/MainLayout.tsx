import { Outlet, NavLink } from 'react-router-dom';
import { LayoutDashboard, Car, Users, LogOut, Settings, Map, Wrench, FileCheck, AlertTriangle, Droplet } from 'lucide-react';
import { authService } from '../services/authService';
import './MainLayout.css';

const MainLayout = () => {
  return (
    <div className="layout-container">
      {/* Sidebar */}
      <aside className="sidebar glass-panel">
        <div className="sidebar-header">
          <div className="logo-icon">
            <Car size={24} color="var(--primary)" />
          </div>
          <h2>FleetOS</h2>
        </div>
        
        <nav className="sidebar-nav">
          <NavLink to="/dashboard" className={({isActive}) => isActive ? 'nav-item active' : 'nav-item'}>
            <LayoutDashboard size={20} />
            <span>Dashboard</span>
          </NavLink>
          
          <NavLink to="/vehicles" className={({isActive}) => isActive ? 'nav-item active' : 'nav-item'}>
            <Car size={20} />
            <span>Vehicles</span>
          </NavLink>
          
          <NavLink to="/drivers" className={({isActive}) => isActive ? 'nav-item active' : 'nav-item'}>
            <Users size={20} />
            <span>Drivers</span>
          </NavLink>
          
          <NavLink to="/trips" className={({isActive}) => isActive ? 'nav-item active' : 'nav-item'}>
            <Map size={20} />
            <span>Trips</span>
          </NavLink>

          <NavLink to="/maintenance" className={({isActive}) => isActive ? 'nav-item active' : 'nav-item'}>
            <Wrench size={20} />
            <span>Maintenance</span>
          </NavLink>

          <NavLink to="/inspections" className={({isActive}) => isActive ? 'nav-item active' : 'nav-item'}>
            <FileCheck size={20} />
            <span>Inspections</span>
          </NavLink>

          <NavLink to="/incidents" className={({isActive}) => isActive ? 'nav-item active' : 'nav-item'}>
            <AlertTriangle size={20} />
            <span>Incidents</span>
          </NavLink>
          
          <NavLink to="/fuel" className={({isActive}) => isActive ? 'nav-item active' : 'nav-item'}>
            <Droplet size={20} />
            <span>Fuel Logs</span>
          </NavLink>
          
          <div className="nav-divider"></div>
          
          <button className="nav-item btn-logout" onClick={() => authService.logout()}>
            <LogOut size={20} />
            <span>Logout</span>
          </button>
        </nav>
      </aside>

      {/* Main Content Area */}
      <main className="main-content">
        <header className="top-header glass-panel">
          <div className="header-search">
             <input type="text" placeholder="Search..." className="search-input" />
          </div>
          <div className="header-actions">
            <button className="icon-btn">
              <Settings size={20} />
            </button>
            <div className="user-avatar">
              <span>A</span>
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
