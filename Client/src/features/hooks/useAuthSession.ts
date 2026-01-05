import { useEffect, useState } from "react";
import type { AuthSession } from "./useAccount";
import { loadSession, AUTH_SESSION_EVENT } from "./useAccount";
import { getAuthToken, AUTH_TOKEN_EVENT } from "../../lib/api/axios";

// Helper function to get session only if token exists
const getValidatedSession = (): AuthSession | null => {
  const token = getAuthToken();
  const loadedSession = loadSession();
  // Only return session if token exists - token is the source of truth for authentication
  return token ? loadedSession : null;
};

export const useAuthSession = () => {
  const [session, setSession] = useState<AuthSession | null>(() => getValidatedSession());

  useEffect(() => {
    const handler = () => {
      // Update session when either token or session changes
      setSession(getValidatedSession());
    };
    
    // Listen to token changes (from axios interceptor, logout, etc.)
    window.addEventListener(AUTH_TOKEN_EVENT, handler);
    // Listen to session changes (from login, register, etc.)
    window.addEventListener(AUTH_SESSION_EVENT, handler);
    // Listen to localStorage changes from other tabs/windows
    window.addEventListener("storage", handler);
    
    return () => {
      window.removeEventListener(AUTH_TOKEN_EVENT, handler);
      window.removeEventListener(AUTH_SESSION_EVENT, handler);
      window.removeEventListener("storage", handler);
    };
  }, []);

  return { session, setSession };
};


