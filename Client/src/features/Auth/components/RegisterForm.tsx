import React, { useState } from "react";
import {
  Alert,
  Box,
  Button,
  Paper,
  Stack,
  TextField,
  Typography
} from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  registerSchema,
  type RegisterFormValues
} from "../../../lib/schemas/loginSchema";
import { handleApiError, useRegister } from "../../hooks/useAccount";
import { Link, useNavigate, useLocation } from "react-router";
import type { Location } from "react-router";

type RegisterFormProps = {
  onSuccess?: () => void;
};

const RegisterForm: React.FC<RegisterFormProps> = ({ onSuccess }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const registerMutation = useRegister();
  const [serverError, setServerError] = useState<string | null>(null);
  const [isSuccess, setIsSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    mode: "onTouched",
    defaultValues: {
      email: "",
      password: "",
      confirmPassword: "",
      displayName: ""
    }
  });

  const onSubmit = async (values: RegisterFormValues) => {
    setServerError(null);
    setIsSuccess(false);
    try {
      const result = await registerMutation.mutateAsync(values);
      if (result.succeeded) {
        setIsSuccess(true);
        const redirectTo =
          ((location.state as { from?: Location })?.from?.pathname) ?? "/videos";
        if (onSuccess) onSuccess();
        setTimeout(() => navigate(redirectTo), 300);
      }
    } catch (error) {
      setServerError(handleApiError(error));
    }
  };

  return (
    <Box sx={{ display: "flex", justifyContent: "center", mt: 8, px: 2 }}>
      <Paper sx={{ p: 4, maxWidth: 520, width: "100%" }} elevation={3}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Create account
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Sign up with your email and password to get started.
        </Typography>

        {serverError && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {serverError}
          </Alert>
        )}
        {isSuccess && (
          <Alert severity="success" sx={{ mb: 2 }}>
            Account created successfully. Redirecting to sign in...
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
              disabled={registerMutation.isPending}
            />
            <TextField
              label="Password"
              type="password"
              fullWidth
              {...register("password")}
              error={!!errors.password}
              helperText={errors.password?.message}
              disabled={registerMutation.isPending}
            />
            <TextField
              label="Confirm password"
              type="password"
              fullWidth
              {...register("confirmPassword")}
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword?.message}
              disabled={registerMutation.isPending}
            />
            <TextField
              label="Display name (optional)"
              fullWidth
              {...register("displayName")}
              error={!!errors.displayName}
              helperText={errors.displayName?.message}
              disabled={registerMutation.isPending}
            />

            <Button
              type="submit"
              variant="contained"
              disabled={registerMutation.isPending}
            >
              {registerMutation.isPending ? "Creating..." : "Create account"}
            </Button>
          </Stack>
        </Box>

        <Typography variant="body2" sx={{ mt: 2 }}>
          Already have an account? <Link to="/login">Sign in</Link>
        </Typography>
      </Paper>
    </Box>
  );
};

export default RegisterForm;

