import { useEffect, useState } from "react";
import type { AuthSession } from "./useAccount";
import { loadSession, AUTH_SESSION_EVENT } from "./useAccount";

export const useAuthSession = () => {
  const [session, setSession] = useState<AuthSession | null>(() => loadSession());

  useEffect(() => {
    const handler = () => setSession(loadSession());
    window.addEventListener("storage", handler);
    window.addEventListener(AUTH_SESSION_EVENT, handler);
    return () => {
      window.removeEventListener("storage", handler);
      window.removeEventListener(AUTH_SESSION_EVENT, handler);
    };
  }, []);

  return { session, setSession };
};


