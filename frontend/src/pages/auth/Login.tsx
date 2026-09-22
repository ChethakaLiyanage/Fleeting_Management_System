import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Car, AlertCircle, Shield, BarChart2, Truck } from 'lucide-react';
import { authService } from '../../services/authService';
import './Login.css';

const Login = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('admin@fleetos.com');
  const [password, setPassword] = useState('admin123');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      await authService.login({ email, password });
      navigate('/dashboard');
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data || err.message || 'Login failed. Please verify credentials.';
      setError(typeof msg === 'string' ? msg : JSON.stringify(msg));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      {/* Left Branding Panel */}
      <div className="login-left-panel">
        <div className="login-brand">
          <div className="login-brand-logo">
            <Car size={36} color="white" />
          </div>
          <h1>Fleet<span>OS</span></h1>
          <p>The complete fleet management platform for modern enterprises. Monitor, manage and optimize your entire fleet.</p>

          <div className="login-features">
            <div className="login-feature-item">
              <div className="login-feature-icon"><Truck size={16} /></div>
              <span>Real-time vehicle tracking and management</span>
            </div>
            <div className="login-feature-item">
              <div className="login-feature-icon"><BarChart2 size={16} /></div>
              <span>Advanced analytics and reporting</span>
            </div>
            <div className="login-feature-item">
              <div className="login-feature-icon"><Shield size={16} /></div>
              <span>Comprehensive incident and safety management</span>
            </div>
          </div>
        </div>
      </div>

      {/* Right Form Panel */}
      <div className="login-right-panel">
        <div className="login-card">
          <div className="login-header">
            <h2>Welcome back</h2>
            <p>Sign in to your administrator account</p>
          </div>

          {error && (
            <div className="login-error">
              <AlertCircle size={15} />
              <span>{error}</span>
            </div>
          )}

          <form onSubmit={handleLogin} className="login-form">
            <div className="form-group">
              <label>Email Address</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="admin@fleetos.com"
                required
                className="form-control"
              />
            </div>

            <div className="form-group">
              <label>Password</label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter your password"
                required
                className="form-control"
              />
            </div>

            <div className="form-options">
              <label className="checkbox-container">
                <input type="checkbox" defaultChecked />
                Remember me
              </label>
              <button type="button" className="forgot-password">Forgot password?</button>
            </div>

            <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
              {loading ? <span className="spinner" /> : 'Sign In'}
            </button>
          </form>

          <div className="login-footer">
            &copy; {new Date().getFullYear()} FleetOS &mdash; Fleet Management System
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
