import React from "react";
import { Box, CircularProgress, Typography } from "@mui/material";

const ConfirmingView: React.FC = () => {
  return (
    <Box sx={{ display: "flex", flexDirection: "column", alignItems: "center", my: 4 }}>
      <CircularProgress sx={{ mb: 2 }} />
      <Typography variant="body2" color="text.secondary">
        Please wait while we confirm your email...
      </Typography>
    </Box>
  );
};

export default ConfirmingView;

