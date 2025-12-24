import React, { useState, useMemo } from "react";
import {
  Alert,
  Box,
  Button,
  Paper,
  Stack,
  TextField,
  Typography,
  Avatar,
  Grid,
  LinearProgress
} from "@mui/material";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  registerSchema,
  type RegisterFormValues
} from "../../../../lib/schemas/loginSchema";
import { handleApiError, useRegister } from "../../../hooks/useAccount";
import { Link, useNavigate, useLocation } from "react-router";
import type { Location } from "react-router";
import { AVAILABLE_AVATARS } from "../../../../lib/constants/avatars";

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
    setValue,
    control,
    formState: { errors }
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    mode: "onChange", // Changed to onChange for real-time validation
    defaultValues: {
      email: "",
      password: "",
      confirmPassword: "",
      displayName: "",
      avatarUrl: AVAILABLE_AVATARS[0] // Default to first avatar
    }
  });

  const selectedAvatar = useWatch({ control, name: "avatarUrl" });
  const password = useWatch({ control, name: "password" });
  const confirmPassword = useWatch({ control, name: "confirmPassword" });

  // Password strength validation
  const passwordRequirements = useMemo(() => {
    if (!password) return null;
    
    return {
      minLength: password.length >= 8,
      hasNumber: /[0-9]/.test(password),
      hasUppercase: /[A-Z]/.test(password)
    };
  }, [password]);

  const passwordStrength = useMemo(() => {
    if (!password) return 0;
    let strength = 0;
    if (password.length >= 8) strength += 33;
    if (/[0-9]/.test(password)) strength += 33;
    if (/[A-Z]/.test(password)) strength += 34;
    return strength;
  }, [password]);

  const getPasswordStrengthColor = () => {
    if (passwordStrength < 33) return "error";
    if (passwordStrength < 66) return "warning";
    return "success";
  };

  const onSubmit = async (values: RegisterFormValues) => {
    setServerError(null);
    setIsSuccess(false);
    try {
      const result = await registerMutation.mutateAsync(values);
      if (result.succeeded) {
        setIsSuccess(true);
        // Only redirect to videos if token is present (email confirmed)
        // If no token, show success message and redirect to login page
        if (result.token) {
          const redirectTo =
            ((location.state as { from?: Location })?.from?.pathname) ?? "/videos";
          if (onSuccess) onSuccess();
          setTimeout(() => navigate(redirectTo), 300);
        } else {
          // Email not confirmed - redirect to check email page
          setTimeout(() => {
            navigate(`/check-email?email=${encodeURIComponent(values.email)}`, { 
              replace: true
            });
          }, 1000);
        }
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
        {isSuccess && !registerMutation.data?.token && (
          <Alert severity="success" sx={{ mb: 2 }}>
            Account created successfully. Redirecting to check your email...
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
            <Box>
              <TextField
                label="Password"
                type="password"
                fullWidth
                {...register("password")}
                error={!!errors.password || (!!password && passwordRequirements !== null && !Object.values(passwordRequirements).every(v => v))}
                helperText={errors.password?.message}
                disabled={registerMutation.isPending}
              />
              {password && (
                <Box sx={{ mt: 1 }}>
                  <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                    <LinearProgress
                      variant="determinate"
                      value={passwordStrength}
                      color={getPasswordStrengthColor()}
                      sx={{ flexGrow: 1, height: 6, borderRadius: 3 }}
                    />
                    <Typography variant="caption" color="text.secondary" sx={{ minWidth: 40 }}>
                      {passwordStrength}%
                    </Typography>
                  </Box>
                  <Box sx={{ mt: 1 }}>
                    <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 0.5 }}>
                      Password requirements:
                    </Typography>
                    <Box component="ul" sx={{ m: 0, pl: 2, fontSize: "0.75rem" }}>
                      <Box
                        component="li"
                        sx={{
                          color: passwordRequirements?.minLength ? "success.main" : "error.main"
                        }}
                      >
                        At least 8 characters
                      </Box>
                      <Box
                        component="li"
                        sx={{
                          color: passwordRequirements?.hasNumber ? "success.main" : "error.main"
                        }}
                      >
                        At least one number
                      </Box>
                      <Box
                        component="li"
                        sx={{
                          color: passwordRequirements?.hasUppercase ? "success.main" : "error.main"
                        }}
                      >
                        At least one uppercase letter
                      </Box>
                    </Box>
                  </Box>
                </Box>
              )}
            </Box>
            <TextField
              label="Confirm password"
              type="password"
              fullWidth
              {...register("confirmPassword")}
              error={!!errors.confirmPassword || (!!confirmPassword && !!password && confirmPassword !== password)}
              helperText={
                errors.confirmPassword?.message ||
                (confirmPassword && password && confirmPassword !== password
                  ? "Passwords do not match"
                  : "")
              }
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

            <Box>
              <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
                Choose your avatar
              </Typography>
              <Grid container spacing={2}>
                {AVAILABLE_AVATARS.map((avatar) => (
                  <Grid  key={avatar}>
                    <Box
                      onClick={() => setValue("avatarUrl", avatar)}
                      sx={{
                        cursor: "pointer",
                        border: selectedAvatar === avatar ? 3 : 1,
                        borderColor: selectedAvatar === avatar ? "primary.main" : "divider",
                        borderRadius: 2,
                        p: 1,
                        display: "flex",
                        justifyContent: "center",
                        alignItems: "center",
                        transition: "all 0.2s",
                        "&:hover": {
                          borderColor: "primary.main",
                          transform: "scale(1.05)"
                        }
                      }}
                    >
                      <Avatar
                        src={avatar}
                        sx={{
                          width: 80,
                          height: 80
                        }}
                      />
                    </Box>
                  </Grid>
                ))}
              </Grid>
            </Box>

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

