import React, { useState, useEffect } from "react";
import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  Paper,
  Stack,
  TextField,
  Typography
} from "@mui/material";
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

type LoginFormProps = {
  onSuccess?: () => void;
};

const LoginForm: React.FC<LoginFormProps> = ({ onSuccess }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const loginMutation = useLogin();
  const [serverError, setServerError] = useState<string | null>(null);
  const [infoMessage, setInfoMessage] = useState<string | null>(null);

  // Check for message from location state (e.g., from registration or email confirmation)
  useEffect(() => {
    const stateMessage = (location.state as { message?: string })?.message;
    if (stateMessage) {
      setInfoMessage(stateMessage);
      // Clear the state to prevent showing the message again on refresh
      window.history.replaceState({}, document.title);
    }
  }, [location]);

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
        const email = (error as any).response?.data?.email || values.email;
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
    <Box sx={{ display: "flex", justifyContent: "center", mt: 8, px: 2 }}>
      <Paper sx={{ p: 4, maxWidth: 420, width: "100%" }} elevation={3}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Sign in
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Enter your account credentials to continue.
        </Typography>

        {infoMessage && (
          <Alert severity="info" sx={{ mb: 2 }} onClose={() => setInfoMessage(null)}>
            {infoMessage}
          </Alert>
        )}
        {serverError && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {serverError}
          </Alert>
        )}

        <Box component="form" noValidate onSubmit={handleSubmit(onSubmit)}>
          <Stack spacing={2}>
            <TextField
              label="Email"
              fullWidth
              {...register("email")}
              error={!!errors.email}
              helperText={errors.email?.message}
              disabled={loginMutation.isPending}
            />
            <TextField
              label="Password"
              type="password"
              fullWidth
              {...register("password")}
              error={!!errors.password}
              helperText={errors.password?.message}
              disabled={loginMutation.isPending}
            />
            <FormControlLabel
              control={
                <Checkbox
                  {...register("rememberMe")}
                  disabled={loginMutation.isPending}
                />
              }
              label="Remember me"
            />
            <Button
              type="submit"
              variant="contained"
              disabled={loginMutation.isPending}
            >
              {loginMutation.isPending ? "Signing in..." : "Sign in"}
            </Button>
          </Stack>
        </Box>

        <Typography variant="body2" sx={{ mt: 2 }}>
          Don&apos;t have an account?{" "}
          <Link to="/signup">Create one</Link>
        </Typography>
      </Paper>
    </Box>
  );
};

export default LoginForm;

