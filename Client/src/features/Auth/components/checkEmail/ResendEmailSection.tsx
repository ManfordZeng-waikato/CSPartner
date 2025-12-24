import React from "react";
import {
  Alert,
  Button,
  Stack,
  TextField
} from "@mui/material";
import type { UseMutationResult } from "@tanstack/react-query";

type ResendEmailSectionProps = {
  email: string;
  onEmailChange: (email: string) => void;
  resendEmailMutation: UseMutationResult<
    { message: string },
    Error,
    string,
    unknown
  >;
  onResend: (email: string) => void;
};

const ResendEmailSection: React.FC<ResendEmailSectionProps> = ({
  email,
  onEmailChange,
  resendEmailMutation,
  onResend
}) => {
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (email) {
      onResend(email);
    }
  };

  return (
    <Stack spacing={2} sx={{ mb: 3 }} component="form" onSubmit={handleSubmit}>
      <TextField
        label="Email"
        type="email"
        fullWidth
        value={email}
        onChange={(e) => onEmailChange(e.target.value)}
        disabled={resendEmailMutation.isPending}
      />
      <Button
        type="submit"
        variant="outlined"
        disabled={!email || resendEmailMutation.isPending}
        fullWidth
      >
        {resendEmailMutation.isPending ? "Sending..." : "Resend Confirmation Email"}
      </Button>
      {resendEmailMutation.isSuccess && (
        <Alert severity="success">
          {resendEmailMutation.data.message}
        </Alert>
      )}
      {resendEmailMutation.isError && (
        <Alert severity="error">
          Failed to resend email. Please try again later.
        </Alert>
      )}
    </Stack>
  );
};

export default ResendEmailSection;

