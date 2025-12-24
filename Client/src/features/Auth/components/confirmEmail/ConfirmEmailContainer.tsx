import React from "react";
import { Box, Paper, Typography } from "@mui/material";
import ConfirmingView from "./ConfirmingView";
import SuccessView from "./SuccessView";
import ErrorView from "./ErrorView";
import type { AuthResult } from "../../../hooks/useAccount";
import type { UseMutationResult } from "@tanstack/react-query";

type ConfirmEmailContainerProps = {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  data?: AuthResult;
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

const ConfirmEmailContainer: React.FC<ConfirmEmailContainerProps> = ({
  isPending,
  isSuccess,
  isError,
  data,
  error,
  email,
  onEmailChange,
  resendEmailMutation,
  onResend
}) => {
  return (
    <Box sx={{ display: "flex", justifyContent: "center", mt: 8, px: 2 }}>
      <Paper sx={{ p: 4, maxWidth: 500, width: "100%" }} elevation={3}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Confirming Your Email
        </Typography>

        {isPending && <ConfirmingView />}

        {isSuccess && data && <SuccessView data={data} />}

        {isError && (
          <ErrorView
            error={error}
            email={email}
            onEmailChange={onEmailChange}
            resendEmailMutation={resendEmailMutation}
            onResend={onResend}
          />
        )}

        {!isPending && !isSuccess && !isError && (
          <Typography variant="body2" color="text.secondary">
            Processing...
          </Typography>
        )}
      </Paper>
    </Box>
  );
};

export default ConfirmEmailContainer;

