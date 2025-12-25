import React, { useState, useEffect } from "react";
import { Box, Button, Stack, TextField, Typography } from "@mui/material";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  resetPasswordSchema,
  type ResetPasswordFormValues
} from "../../lib/schemas/loginSchema";
import { handleApiError, useResetPassword } from "../hooks/useAccount";
import { Link, useNavigate, useSearchParams } from "react-router";
import { FormContainer, FormTextField, PasswordFieldWithStrength } from "../../app/shared/components";

const ResetPasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const resetPasswordMutation = useResetPassword();
  const [serverError, setServerError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Get email and code from URL parameters
  const emailFromUrl = searchParams.get("email") || "";
  const codeFromUrl = searchParams.get("code") || "";

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors }
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    mode: "onChange", // Changed to onChange for real-time validation
    defaultValues: {
      email: emailFromUrl,
      newPassword: "",
      confirmPassword: "",
      code: codeFromUrl
    }
  });

  const newPassword = useWatch({ control, name: "newPassword" });
  const confirmPassword = useWatch({ control, name: "confirmPassword" });

  // Update form values when URL params change - use reset to avoid cascading renders
  useEffect(() => {
    if (emailFromUrl || codeFromUrl) {
      reset({
        email: emailFromUrl,
        newPassword: "",
        confirmPassword: "",
        code: codeFromUrl
      });
    }
  }, [emailFromUrl, codeFromUrl, reset]);

  const onSubmit = async (values: ResetPasswordFormValues) => {
    setServerError(null);
    setSuccessMessage(null);
    try {
      const result = await resetPasswordMutation.mutateAsync({
        email: values.email,
        newPassword: values.newPassword,
        confirmPassword: values.confirmPassword,
        code: values.code
      });
      setSuccessMessage(result.message);
      // Redirect to login page after 2 seconds
      setTimeout(() => {
        navigate("/login", {
          state: { message: "Password reset successful. Please login with your new password." }
        });
      }, 2000);
    } catch (error) {
      setServerError(handleApiError(error));
    }
  };

  // Show error if required parameters are missing
  if (!emailFromUrl || !codeFromUrl) {
    return (
      <FormContainer
        title="Reset Password"
        errorMessage="Invalid reset link. Please request a new password reset."
      >
        <Typography variant="body2" sx={{ mt: 2, textAlign: "center" }}>
          <Link to="/forgot-password">Request new reset link</Link>
        </Typography>
      </FormContainer>
    );
  }

  const successAlertMessage = successMessage ? (
    <>
      {successMessage}
      <br />
      <Typography variant="body2" sx={{ mt: 1 }}>
        Redirecting to login page...
      </Typography>
    </>
  ) : null;

  return (
    <FormContainer
      title="Reset Password"
      subtitle="Enter your new password below."
      errorMessage={serverError}
      successMessage={successAlertMessage}
      onCloseError={() => setServerError(null)}
    >
      {!successMessage && (
        <Box component="form" noValidate onSubmit={handleSubmit(onSubmit)}>
          <Stack spacing={2}>
            <FormTextField
              label="Email"
              type="email"
              fullWidth
              register={register("email")}
              error={errors.email}
              disabled={true}
            />
            <PasswordFieldWithStrength
              register={register("newPassword")}
              control={control}
              passwordFieldName="newPassword"
              label="New Password"
              error={errors.newPassword}
              disabled={resetPasswordMutation.isPending}
            />
            <FormTextField
              label="Confirm Password"
              type="password"
              fullWidth
              register={register("confirmPassword")}
              error={errors.confirmPassword}
              helperText={
                confirmPassword && newPassword && confirmPassword !== newPassword
                  ? "Passwords do not match"
                  : undefined
              }
              disabled={resetPasswordMutation.isPending}
            />
            <TextField
              label="Reset Code"
              fullWidth
              {...register("code")}
              error={!!errors.code}
              helperText={errors.code?.message || "This code was sent to your email"}
              disabled={true}
              sx={{ display: "none" }}
            />
            <Button
              type="submit"
              variant="contained"
              disabled={resetPasswordMutation.isPending}
            >
              {resetPasswordMutation.isPending ? "Resetting..." : "Reset Password"}
            </Button>
          </Stack>
        </Box>
      )}

      <Typography variant="body2" sx={{ mt: 2, textAlign: "center" }}>
        <Link to="/login">Back to Sign in</Link>
      </Typography>
    </FormContainer>
  );
};

export default ResetPasswordPage;

