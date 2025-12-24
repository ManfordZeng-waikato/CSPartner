import { useMutation, useQueryClient } from "@tanstack/react-query";
import apiClient, { setAuthToken } from "../../lib/api/axios";
import type {
  LoginFormValues,
  RegisterFormValues
} from "../../lib/schemas/loginSchema";

export type AuthResult = {
  succeeded: boolean;
  userId?: string;
  email?: string;
  displayName?: string;
  token?: string;
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

export const useLogin = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (payload: LoginFormValues): Promise<AuthResult> => {
      const response = await apiClient.post<AuthResult>(
        "/api/account/login",
        payload
      );

      if (!response.data.succeeded) {
        // Create error with email information for EMAIL_NOT_CONFIRMED case
        const error = new Error(response.data.errors?.[0] ?? "Login failed");
        if (response.data.errors?.[0] === "EMAIL_NOT_CONFIRMED" && response.data.email) {
          (error as any).email = response.data.email;
          (error as any).response = { data: { email: response.data.email } };
        }
        throw error;
      }
      // Save JWT token
      if (response.data.token) {
        setAuthToken(response.data.token);
      }
      // Persist minimal session info for UI
      const userId = response.data.userId ?? "";
      saveSession({
        userId,
        email: response.data.email,
        displayName: response.data.displayName
      });
      
      // Invalidate and refetch user profile to get latest avatar
      if (userId) {
        await queryClient.invalidateQueries({ queryKey: ['userProfile', userId] });
        // Force refetch immediately
        await queryClient.refetchQueries({ queryKey: ['userProfile', userId] });
      }
      
      return response.data;
    },
    onError: (error) => {
      console.error("Login failed:", error);
    }
  });
};

export const useRegister = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async (payload: RegisterFormValues): Promise<AuthResult> => {
      const response = await apiClient.post<AuthResult>(
        "/api/account/register",
        payload
      );

      if (!response.data.succeeded) {
        throw new Error(response.data.errors?.[0] ?? "Registration failed");
      }
      
      // Only save token and session if token is present (email confirmed)
      // If token is null, user needs to confirm email before logging in
      if (response.data.token) {
        setAuthToken(response.data.token);
        // Persist minimal session info for UI only when token exists
        const userId = response.data.userId ?? "";
        saveSession({
          userId,
          email: response.data.email,
          displayName: response.data.displayName
        });
        
        // Invalidate and refetch user profile to get latest avatar
        if (userId) {
          await queryClient.invalidateQueries({ queryKey: ['userProfile', userId] });
          // Force refetch immediately
          await queryClient.refetchQueries({ queryKey: ['userProfile', userId] });
        }
      }
      // If no token, don't save session - user needs to confirm email first
      
      return response.data;
    },
    onError: (error) => {
      console.error("Register failed:", error);
    }
  });
};

export const useLogout = () =>
  useMutation({
    mutationFn: async (): Promise<void> => {
      await apiClient.post("/api/account/logout", null);
      setAuthToken(null);
      clearSession();
    },
    onError: (error) => {
      console.error("Logout failed:", error);
      // Clear token even if logout request fails
      setAuthToken(null);
      clearSession();
    }
  });

export const useConfirmEmail = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async ({ userId, code }: { userId: string; code: string }): Promise<AuthResult> => {
      const response = await apiClient.post<AuthResult>(
        `/api/account/confirmEmail?userId=${userId}&code=${encodeURIComponent(code)}`
      );

      if (!response.data.succeeded) {
        throw new Error(response.data.errors?.[0] ?? "Email confirmation failed");
      }

      // Save JWT token if present (for auto-login after confirmation)
      if (response.data.token) {
        setAuthToken(response.data.token);
      }
      
      // Persist session info for UI
      const userIdFromResponse = response.data.userId ?? "";
      if (userIdFromResponse) {
        saveSession({
          userId: userIdFromResponse,
          email: response.data.email,
          displayName: response.data.displayName
        });
        
        // Invalidate and refetch user profile to get latest avatar
        await queryClient.invalidateQueries({ queryKey: ['userProfile', userIdFromResponse] });
        await queryClient.refetchQueries({ queryKey: ['userProfile', userIdFromResponse] });
      }
      
      return response.data;
    },
    onError: (error) => {
      console.error("Email confirmation failed:", error);
    }
  });
};

export const useResendConfirmationEmail = () => {
  return useMutation({
    mutationFn: async (email: string): Promise<{ message: string }> => {
      const response = await apiClient.get<{ message: string }>(
        `/api/account/resendConfirmationEmail?email=${encodeURIComponent(email)}`
      );
      return response.data;
    },
    onError: (error) => {
      console.error("Resend confirmation email failed:", error);
    }
  });
};

export const handleApiError = (error: unknown): string =>
  extractErrorMessage(error);

export { AUTH_SESSION_EVENT };
