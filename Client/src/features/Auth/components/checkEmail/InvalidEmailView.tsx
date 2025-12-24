import React from "react";
import { Link } from "react-router";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";

const InvalidEmailView: React.FC = () => {
  return (
    <Box sx={{ display: "flex", justifyContent: "center", mt: 8, px: 2 }}>
      <Paper sx={{ p: 4, maxWidth: 500, width: "100%" }} elevation={3}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Invalid Request
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          No email address provided. Please register again.
        </Typography>
        <Stack spacing={2}>
          <Button variant="contained" component={Link} to="/signup">
            Go to Sign Up
          </Button>
          <Button variant="outlined" component={Link} to="/login">
            Back to Login
          </Button>
        </Stack>
      </Paper>
    </Box>
  );
};

export default InvalidEmailView;

