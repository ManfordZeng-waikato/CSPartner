import React from "react";
import { Alert, Stack, Typography } from "@mui/material";
import type { AuthResult } from "../../../hooks/useAccount";

type SuccessViewProps = {
  data: AuthResult;
};

const SuccessView: React.FC<SuccessViewProps> = ({ data }) => {
  return (
    <Stack spacing={2}>
      <Alert severity="success" sx={{ mb: 2 }}>
        {data.token
          ? "Email confirmed successfully! You are being automatically logged in..."
          : "Email confirmed successfully! Please log in to your account."}
      </Alert>
      {data.token && (
        <Typography variant="body2" color="text.secondary" sx={{ textAlign: "center" }}>
          Redirecting to home page...
        </Typography>
      )}
    </Stack>
  );
};

export default SuccessView;

