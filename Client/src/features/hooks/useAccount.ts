import { useMutation } from "@tanstack/react-query";
import axios from "axios";
import type {
  LoginFormValues,
  RegisterFormValues
} from "../../lib/schemas/loginSchema";

export type AuthResult = {
  succeeded: boolean;
  userId?: string;
  email?: string;
  displayName?: string;
  errors?: string[];
};

export type AuthSession = Pick<AuthResult, "userId" | "email" | "displayName">;

export const AUTH_SESSION_KEY = "auth_session";
const AUTH_SESSION_EVENT = "auth-session-changed";

export const loadSession = (): AuthSession | null => {
  if (typeof window === "undefined") return null;
  const raw = localStorage.getItem(AUTH_SESSION_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthSession;
  } catch {
    return null;
  }
};

export const saveSession = (session: AuthSession) => {
  if (typeof window === "undefined") return;
  localStorage.setItem(AUTH_SESSION_KEY, JSON.stringify(session));
  window.dispatchEvent(new Event(AUTH_SESSION_EVENT));
};

export const clearSession = () => {
  if (typeof window === "undefined") return;
  localStorage.removeItem(AUTH_SESSION_KEY);
  window.dispatchEvent(new Event(AUTH_SESSION_EVENT));
};

type ApiErrorData = { errors?: string[]; error?: string };
type ApiErrorResponse = { response?: { data?: ApiErrorData } };

const isApiErrorResponse = (value: unknown): value is ApiErrorResponse =>
  typeof value === "object" &&
  value !== null &&
  "response" in value &&
  typeof (value as { response?: unknown }).response === "object";

const extractErrorMessage = (error: unknown) => {
  if (isApiErrorResponse(error) && error.response?.data) {
    const { errors, error: singleError } = error.response.data;
    if (Array.isArray(errors) && errors.length > 0) return errors[0];
    if (typeof singleError === "string") return singleError;
  }
  if (error instanceof Error && error.message) return error.message;
  return "Request failed, please try again later";
};

export const useLogin = () =>
  useMutation({
    mutationFn: async (payload: LoginFormValues): Promise<AuthResult> => {
      const response = await axios.post<AuthResult>(
        "/api/account/login",
        payload,
        { withCredentials: true }
      );

      if (!response.data.succeeded) {
        throw new Error(response.data.errors?.[0] ?? "Login failed");
      }
      // Persist minimal session info for UI
      saveSession({
        userId: response.data.userId ?? "",
        email: response.data.email,
        displayName: response.data.displayName
      });
      return response.data;
    },
    onError: (error) => {
      console.error("Login failed:", error);
    }
  });

export const useRegister = () =>
  useMutation({
    mutationFn: async (payload: RegisterFormValues): Promise<AuthResult> => {
      const response = await axios.post<AuthResult>(
        "/api/account/register",
        payload,
        { withCredentials: true }
      );

      if (!response.data.succeeded) {
        throw new Error(response.data.errors?.[0] ?? "Registration failed");
      }
      return response.data;
    },
    onError: (error) => {
      console.error("Register failed:", error);
    }
  });

export const useLogout = () =>
  useMutation({
    mutationFn: async (): Promise<void> => {
      await axios.post("/api/account/logout", null, { withCredentials: true });
      clearSession();
    },
    onError: (error) => {
      console.error("Logout failed:", error);
    }
  });

export const handleApiError = (error: unknown): string =>
  extractErrorMessage(error);

export { AUTH_SESSION_EVENT };
