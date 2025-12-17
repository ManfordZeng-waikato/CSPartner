import React, { useState } from "react";
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
} from "../../../lib/schemas/loginSchema";
import { handleApiError, useLogin } from "../../hooks/useAccount";
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
        const redirectTo =
          ((location.state as { from?: Location })?.from?.pathname) ?? "/videos";
        if (onSuccess) onSuccess();
        navigate(redirectTo);
      }
    } catch (error) {
      setServerError(handleApiError(error));
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

