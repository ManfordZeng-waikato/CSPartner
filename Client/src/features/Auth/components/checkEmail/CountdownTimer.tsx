import React from "react";
import { Box, CircularProgress, Typography } from "@mui/material";

type CountdownTimerProps = {
  countdown: number;
};

const CountdownTimer: React.FC<CountdownTimerProps> = ({ countdown }) => {
  return (
    <Box sx={{ display: "flex", alignItems: "center", justifyContent: "center", mb: 3 }}>
      <CircularProgress size={20} sx={{ mr: 1 }} />
      <Typography variant="body2" color="text.secondary">
        Please wait {countdown} seconds before resending...
      </Typography>
    </Box>
  );
};

export default CountdownTimer;

