import React, { useState } from "react";
import {
  Box,
  Button,
  Divider,
  Stack,
  Typography,
  Avatar,
  Grid
} from "@mui/material";
import GitHubIcon from "@mui/icons-material/GitHub";
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
import { FormContainer, FormTextField, PasswordFieldWithStrength } from "../../../../app/shared/components";

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

  const handleGitHubLogin = () => {
    // Get API base URL from environment or use current origin
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || window.location.origin;
    // Redirect to backend GitHub OAuth login endpoint
    // The backend will handle the entire OAuth flow using ASP.NET Core authentication middleware
    // This ensures proper state management and security
    window.location.href = `${apiBaseUrl}/api/account/github-login`;
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

  const successMessage = isSuccess && !registerMutation.data?.token
    ? "Account created successfully. Redirecting to check your email..."
    : null;

  return (
    <FormContainer
      title="Create account"
      subtitle="Sign up with your email and password to get started."
      maxWidth={520}
      errorMessage={serverError}
      successMessage={successMessage}
    >
      <Box component="form" noValidate onSubmit={handleSubmit(onSubmit)}>
        <Stack spacing={2}>
          <FormTextField
            label="Email"
            fullWidth
            register={register("email")}
            error={errors.email}
            disabled={registerMutation.isPending}
          />
          <PasswordFieldWithStrength
            register={register("password")}
            control={control}
            passwordFieldName="password"
            label="Password"
            error={errors.password}
            disabled={registerMutation.isPending}
          />
          <FormTextField
            label="Confirm password"
            type="password"
            fullWidth
            register={register("confirmPassword")}
            error={errors.confirmPassword}
            helperText={
              confirmPassword && password && confirmPassword !== password
                ? "Passwords do not match"
                : undefined
            }
            disabled={registerMutation.isPending}
          />
          <FormTextField
            label="Display name (optional)"
            fullWidth
            register={register("displayName")}
            error={errors.displayName}
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
            fullWidth
          >
            {registerMutation.isPending ? "Creating..." : "Create account"}
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
            disabled={registerMutation.isPending}
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
        Already have an account? <Link to="/login">Sign in</Link>
      </Typography>
    </FormContainer>
  );
};

export default RegisterForm;

