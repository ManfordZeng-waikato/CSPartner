import axios from "axios";

const AUTH_TOKEN_KEY = "auth_token";

export const getAuthToken = (): string | null => {
  if (typeof window === "undefined") return null;
  return localStorage.getItem(AUTH_TOKEN_KEY);
};

export const setAuthToken = (token: string | null) => {
  if (typeof window === "undefined") return;
  if (token) {
    localStorage.setItem(AUTH_TOKEN_KEY, token);
  } else {
    localStorage.removeItem(AUTH_TOKEN_KEY);
  }
};

// Configure axios instance
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || "",
  withCredentials: false, // We're using JWT tokens instead of cookies
  timeout: 300000 // 5 minutes timeout for video uploads
});

// Request interceptor to add JWT token
apiClient.interceptors.request.use(
  (config) => {
    const token = getAuthToken();
    
    // List of endpoints that don't require authentication
    const anonymousEndpoints = [
      '/api/videos',
      '/api/account/login',
      '/api/account/register',
      '/api/userprofiles'
    ];
    
    const isAnonymousEndpoint = config.url && anonymousEndpoints.some(
      endpoint => config.url?.startsWith(endpoint) || config.url?.includes(endpoint)
    );
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    } else if (!isAnonymousEndpoint) {
      // Only warn for endpoints that typically require authentication
      console.warn("No auth token found for request:", config.url);
    }
    
    return config;
  },
  (error) => {
    console.error("Request interceptor error:", error);
    return Promise.reject(error);
  }
);

// Response interceptor to handle 401 errors
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Log error for debugging
    if (error.response) {
      // The request was made and the server responded with a status code
      // that falls out of the range of 2xx
      console.error("API Error:", error.response.status, error.response.data);
      
      if (error.response.status === 401) {
        // Token expired or invalid, clear it
        setAuthToken(null);
        // Don't redirect immediately for upload requests to allow error handling
        // Only redirect for non-upload API calls
        const isUploadRequest = error.config?.url?.includes('/upload');
        if (typeof window !== "undefined" && !isUploadRequest) {
          window.location.href = "/login";
        }
      }
    } else if (error.request) {
      // The request was made but no response was received
      console.error("API Request Error:", error.request);
    } else {
      // Something happened in setting up the request that triggered an Error
      console.error("API Error:", error.message);
    }
    return Promise.reject(error);
  }
);

export default apiClient;

