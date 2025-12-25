import React, { useState } from "react";
import { Button, Stack, Typography } from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  requestPasswordResetSchema,
  type RequestPasswordResetFormValues
} from "../../lib/schemas/loginSchema";
import { handleApiError, useRequestPasswordReset } from "../hooks/useAccount";
import { Link, useNavigate } from "react-router";
import { FormContainer, FormTextField } from "../../app/shared/components";
import { Box } from "@mui/material";

const ForgotPasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const requestPasswordResetMutation = useRequestPasswordReset();
  const [serverError, setServerError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<RequestPasswordResetFormValues>({
    resolver: zodResolver(requestPasswordResetSchema),
    mode: "onTouched",
    defaultValues: {
      email: ""
    }
  });

  const onSubmit = async (values: RequestPasswordResetFormValues) => {
    setServerError(null);
    setSuccessMessage(null);
    try {
      const result = await requestPasswordResetMutation.mutateAsync(values.email);
      setSuccessMessage(result.message);
    } catch (error) {
      const errorMessage = handleApiError(error);
      
      // Check if error is due to unconfirmed email
      // Backend returns: "This email address has not been confirmed. Please confirm your email first before resetting your password."
      if (errorMessage.includes("not been confirmed") || errorMessage.includes("confirm your email")) {
        // Redirect to check email page with the email address
        navigate(`/check-email?email=${encodeURIComponent(values.email)}&autoSend=true`, {
          replace: true
        });
        return;
      }
      
      setServerError(errorMessage);
      // Clear success message if there's an error
      setSuccessMessage(null);
    }
  };

  return (
    <FormContainer
      title="Forgot Password"
      subtitle="Enter your email address and we'll send you a link to reset your password."
      errorMessage={serverError}
      successMessage={successMessage}
      onCloseError={() => setServerError(null)}
      onCloseSuccess={() => setSuccessMessage(null)}
    >
      <Box component="form" noValidate onSubmit={handleSubmit(onSubmit)}>
        <Stack spacing={2}>
          <FormTextField
            label="Email"
            type="email"
            fullWidth
            register={register("email")}
            error={errors.email}
            disabled={requestPasswordResetMutation.isPending || !!successMessage}
          />
          <Button
            type="submit"
            variant="contained"
            disabled={requestPasswordResetMutation.isPending || !!successMessage}
          >
            {requestPasswordResetMutation.isPending ? "Sending..." : "Send Reset Link"}
          </Button>
        </Stack>
      </Box>

      <Typography variant="body2" sx={{ mt: 2, textAlign: "center" }}>
        <Link to="/login">Back to Sign in</Link>
      </Typography>
    </FormContainer>
  );
};

export default ForgotPasswordPage;

