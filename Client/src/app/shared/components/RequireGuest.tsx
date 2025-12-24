import React from "react";
import { Navigate } from "react-router";
import { getAuthToken } from "../../../lib/api/axios";

type RequireGuestProps = {
  children: React.ReactNode;
};

/**
 * Component that redirects authenticated users away from guest-only pages
 * (like login, signup, email confirmation pages)
 */
const RequireGuest: React.FC<RequireGuestProps> = ({ children }) => {
  // Check for JWT token in localStorage
  const token = getAuthToken();
  const isAuthenticated = !!token;

  // If user is already authenticated, redirect to home page
  if (isAuthenticated) {
    return <Navigate to="/videos" replace />;
  }

  // If user is not authenticated, show the guest page
  return <>{children}</>;
};

export default RequireGuest;

