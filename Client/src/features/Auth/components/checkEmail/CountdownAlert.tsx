import React from "react";
import { Alert, Typography } from "@mui/material";

type CountdownAlertProps = {
  canResend: boolean;
  countdown: number;
};

const CountdownAlert: React.FC<CountdownAlertProps> = ({ canResend, countdown }) => {
  return (
    <Alert severity="info" sx={{ mb: 3 }}>
      <Typography variant="body2">
        <strong>Didn't receive the email?</strong>
        {canResend ? (
          <span> You can resend it now.</span>
        ) : (
          <span> You can resend it in {countdown} seconds.</span>
        )}
      </Typography>
    </Alert>
  );
};

export default CountdownAlert;

