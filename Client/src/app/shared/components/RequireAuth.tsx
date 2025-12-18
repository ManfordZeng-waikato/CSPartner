import React from "react";
import { Navigate, useLocation } from "react-router";
import { getAuthToken } from "../../../lib/api/axios";

type RequireAuthProps = {
  children: React.ReactNode;
};

const RequireAuth: React.FC<RequireAuthProps> = ({ children }) => {
  const location = useLocation();
  
  // Check for JWT token in localStorage
  const token = getAuthToken();
  const isAuthenticated = !!token;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <>{children}</>;
};

export default RequireAuth;

