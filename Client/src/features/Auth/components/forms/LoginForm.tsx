import React, { useState, useEffect, useMemo } from "react";
import {
  Box,
  Button,
  Checkbox,
  Divider,
  FormControlLabel,
  Stack,
  Typography
} from "@mui/material";
import GitHubIcon from "@mui/icons-material/GitHub";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  loginSchema,
  type LoginFormValues
} from "../../../../lib/schemas/loginSchema";
import { handleApiError, useLogin } from "../../../hooks/useAccount";
import { processPendingLikes } from "../../../hooks/useVideos";
import { Link, useNavigate, useLocation } from "react-router";
import type { Location } from "react-router";
import { FormContainer, FormTextField } from "../../../../app/shared/components";

type LoginFormProps = {
  onSuccess?: () => void;
};

const LoginForm: React.FC<LoginFormProps> = ({ onSuccess }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const loginMutation = useLogin();
  const [serverError, setServerError] = useState<string | null>(null);
  
  // Get message from location state - compute directly instead of using effect
  const stateMessage = (location.state as { message?: string })?.message;
  const [infoMessage, setInfoMessage] = useState<string | null>(stateMessage || null);

  // Check for error in URL query parameters (from GitHub OAuth callback)
  const searchParams = useMemo(() => new URLSearchParams(location.search), [location.search]);
  const errorParam = useMemo(() => searchParams.get('error'), [searchParams]);
  
  // Initialize error from URL on mount only
  useEffect(() => {
    if (errorParam) {
      const errorMessage = decodeURIComponent(errorParam);
      // Use setTimeout to avoid synchronous setState in effect
      setTimeout(() => {
        setServerError(errorMessage);
      }, 0);
      
      // Clean up URL by removing error parameter
      const newSearchParams = new URLSearchParams(searchParams);
      newSearchParams.delete('error');
      const newSearch = newSearchParams.toString();
      const newUrl = newSearch 
        ? `${location.pathname}?${newSearch}` 
        : location.pathname;
      window.history.replaceState({}, '', newUrl);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Only run on mount

  // Clear location state after reading it to prevent showing the message again on refresh
  useEffect(() => {
    if (stateMessage) {
      window.history.replaceState({}, document.title);
    }
  }, [stateMessage]);

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    mode: "onTouched",
    defaultValues: {
      email: "",
      password: "",
      rememberMe: false
    }
  });

  const handleGitHubLogin = () => {
    // Get API base URL from environment or use current origin
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || window.location.origin;
    // Redirect to backend GitHub OAuth login endpoint
    // The backend will handle the entire OAuth flow using ASP.NET Core authentication middleware
    // This ensures proper state management and security
    window.location.href = `${apiBaseUrl}/api/account/github-login`;
  };

  const onSubmit = async (values: LoginFormValues) => {
    setServerError(null);
    try {
      const result = await loginMutation.mutateAsync(values);
      if (result.succeeded) {
        // Process any pending likes from before login
        await processPendingLikes();
        
        // Check for return URL in state or search params
        const stateFrom = (location.state as { from?: Location })?.from;
        const searchParams = new URLSearchParams(location.search);
        const returnUrl = searchParams.get('returnUrl');
        
        // Use stateFrom pathname (which may include hash) or returnUrl or default
        const redirectTo = stateFrom?.pathname || returnUrl || "/videos";
        
        if (onSuccess) onSuccess();
        
        // Navigate to the redirect URL (hash is included in the pathname)
        navigate(redirectTo);
      }
    } catch (error) {
      // Check if error is due to unconfirmed email
      if (error instanceof Error && error.message === "EMAIL_NOT_CONFIRMED") {
        // Get email from the error response or use the form email
        const email = (error as { response?: { data?: { email?: string } } }).response?.data?.email || values.email;
        // Redirect to check email page with autoSend flag
        navigate(`/check-email?email=${encodeURIComponent(email)}&autoSend=true`, {
          replace: true
        });
      } else {
        setServerError(handleApiError(error));
      }
    }
  };

  return (
    <FormContainer
      title="Sign in"
      subtitle="Enter your account credentials to continue."
      errorMessage={serverError}
      infoMessage={infoMessage}
      onCloseInfo={() => setInfoMessage(null)}
    >
      <Box component="form" noValidate onSubmit={handleSubmit(onSubmit)}>
        <Stack spacing={2}>
          <FormTextField
            label="Email"
            fullWidth
            register={register("email")}
            error={errors.email}
            disabled={loginMutation.isPending}
          />
          <FormTextField
            label="Password"
            type="password"
            fullWidth
            register={register("password")}
            error={errors.password}
            disabled={loginMutation.isPending}
          />
          <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
            <FormControlLabel
              control={
                <Checkbox
                  {...register("rememberMe")}
                  disabled={loginMutation.isPending}
                />
              }
              label="Remember me"
            />
            <Link
              to="/forgot-password"
              style={{ textDecoration: "none", fontSize: "0.875rem" }}
            >
              Forgot password?
            </Link>
          </Box>
          <Button
            type="submit"
            variant="contained"
            disabled={loginMutation.isPending}
            fullWidth
          >
            {loginMutation.isPending ? "Signing in..." : "Sign in"}
          </Button>
          
          <Divider sx={{ my: 2 }}>
            <Typography variant="body2" color="text.secondary">
              OR
            </Typography>
          </Divider>
          
          <Button
            type="button"
            variant="outlined"
            fullWidth
            startIcon={<GitHubIcon />}
            onClick={handleGitHubLogin}
            disabled={loginMutation.isPending}
            sx={{
              textTransform: "none",
              py: 1.5
            }}
          >
            Continue with GitHub
          </Button>
        </Stack>
      </Box>

      <Typography variant="body2" sx={{ mt: 2 }}>
        Don&apos;t have an account?{" "}
        <Link to="/signup">Create one</Link>
      </Typography>
    </FormContainer>
  );
};

export default LoginForm;

