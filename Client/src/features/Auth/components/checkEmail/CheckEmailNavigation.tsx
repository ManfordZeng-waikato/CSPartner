import React from "react";
import { Link } from "react-router";
import { Button, Stack, Typography } from "@mui/material";

const CheckEmailNavigation: React.FC = () => {
  return (
    <Stack spacing={2}>
      <Typography variant="body2" color="text.secondary" sx={{ textAlign: "center" }}>
        Already confirmed your email?
      </Typography>
      <Button variant="contained" component={Link} to="/login" fullWidth>
        Go to Login
      </Button>
      <Button variant="outlined" component={Link} to="/signup" fullWidth>
        Create Another Account
      </Button>
    </Stack>
  );
};

export default CheckEmailNavigation;

