import React from "react";
import { Box, Paper, Typography } from "@mui/material";
import CountdownAlert from "./CountdownAlert";
import CountdownTimer from "./CountdownTimer";
import ResendEmailSection from "./ResendEmailSection";
import CheckEmailNavigation from "./CheckEmailNavigation";
import type { UseMutationResult } from "@tanstack/react-query";

type CheckEmailContainerProps = {
  email: string;
  countdown: number;
  canResend: boolean;
  resendEmail: string;
  onResendEmailChange: (email: string) => void;
  resendEmailMutation: UseMutationResult<
    { message: string },
    Error,
    string,
    unknown
  >;
  onResend: (email: string) => void;
};

const CheckEmailContainer: React.FC<CheckEmailContainerProps> = ({
  email,
  countdown,
  canResend,
  resendEmail,
  onResendEmailChange,
  resendEmailMutation,
  onResend
}) => {
  return (
    <Box sx={{ display: "flex", justifyContent: "center", mt: 8, px: 2 }}>
      <Paper sx={{ p: 4, maxWidth: 500, width: "100%" }} elevation={3}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Check Your Email
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          We've sent a confirmation email to <strong>{email}</strong>. Please check your inbox and click the confirmation link to verify your account.
        </Typography>

        <CountdownAlert canResend={canResend} countdown={countdown} />

        {!canResend && <CountdownTimer countdown={countdown} />}

        {canResend && (
          <ResendEmailSection
            email={resendEmail}
            onEmailChange={onResendEmailChange}
            resendEmailMutation={resendEmailMutation}
            onResend={onResend}
          />
        )}

        <CheckEmailNavigation />
      </Paper>
    </Box>
  );
};

export default CheckEmailContainer;

