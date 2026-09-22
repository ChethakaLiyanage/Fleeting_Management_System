import axios from 'axios';

// Note: Ensure this matches the port your .NET backend is running on (e.g., 5241 or 7198)
const API_BASE_URL = 'http://localhost:5241/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor
api.interceptors.request.use(
  (config) => {
    // We can add auth tokens here later
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response Interceptor
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // Global error handling
    return Promise.reject(error);
  }
);
