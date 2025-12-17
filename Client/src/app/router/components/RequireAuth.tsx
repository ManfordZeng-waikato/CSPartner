import React from "react";
import { Navigate, useLocation } from "react-router";
import { useAuthSession } from "../../../features/hooks/useAuthSession";

type RequireAuthProps = {
  children: React.ReactNode;
};

const RequireAuth: React.FC<RequireAuthProps> = ({ children }) => {
  const location = useLocation();
  const { session } = useAuthSession();

  const hasCookie = typeof document !== "undefined"
    ? document.cookie.includes("Identity.Application")
    : false;

  const isAuthed = !!session || hasCookie;

  if (!isAuthed) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <>{children}</>;
};

export default RequireAuth;

