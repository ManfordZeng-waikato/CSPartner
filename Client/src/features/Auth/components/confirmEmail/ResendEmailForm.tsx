import React from "react";
import {
  Alert,
  Box,
  Button,
  Stack,
  TextField,
  Typography
} from "@mui/material";
import type { UseMutationResult } from "@tanstack/react-query";

type ResendEmailFormProps = {
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

const ResendEmailForm: React.FC<ResendEmailFormProps> = ({
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
    <Box>
      <Typography variant="body2" sx={{ mb: 2 }}>
        Didn't receive the confirmation email?
      </Typography>
      <Stack spacing={2} component="form" onSubmit={handleSubmit}>
        <TextField
          label="Email"
          type="email"
          fullWidth
          value={email || ""}
          onChange={(e) => onEmailChange(e.target.value)}
          disabled={resendEmailMutation.isPending}
        />
        <Button
          type="submit"
          variant="outlined"
          disabled={!email || resendEmailMutation.isPending}
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
    </Box>
  );
};

export default ResendEmailForm;

