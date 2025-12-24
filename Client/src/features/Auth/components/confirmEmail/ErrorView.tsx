import React from "react";
import { Link } from "react-router";
import {
  Alert,
  Button,
  Stack
} from "@mui/material";
import ResendEmailForm from "./ResendEmailForm";
import type { UseMutationResult } from "@tanstack/react-query";

type ErrorViewProps = {
  error: Error | null;
  email: string | null;
  onEmailChange: (email: string) => void;
  resendEmailMutation: UseMutationResult<
    { message: string },
    Error,
    string,
    unknown
  >;
  onResend: (email: string) => void;
};

const ErrorView: React.FC<ErrorViewProps> = ({
  error,
  email,
  onEmailChange,
  resendEmailMutation,
  onResend
}) => {
  const errorMessage =
    error instanceof Error
      ? error.message
      : "Failed to confirm email. The link may be invalid or expired.";

  return (
    <Stack spacing={2}>
      <Alert severity="error">{errorMessage}</Alert>
      <ResendEmailForm
        email={email}
        onEmailChange={onEmailChange}
        resendEmailMutation={resendEmailMutation}
        onResend={onResend}
      />
      <Stack direction="row" spacing={2}>
        <Button variant="contained" component={Link} to="/login">
          Back to Login
        </Button>
        <Button variant="outlined" component={Link} to="/signup">
          Create New Account
        </Button>
      </Stack>
    </Stack>
  );
};

export default ErrorView;

