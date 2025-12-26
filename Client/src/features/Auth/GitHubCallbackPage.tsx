import React, { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router";
import { Box, CircularProgress, Typography, Alert } from "@mui/material";
import { saveSession } from "../hooks/useAccount";
import { setAuthToken } from "../../lib/api/axios";
import { processPendingLikes } from "../hooks/useVideos";
import { useQueryClient } from "@tanstack/react-query";

const GitHubCallbackPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const handleCallback = async () => {
      try {
        // Extract token and user info from URL query parameters
        const token = searchParams.get("token");
        const userId = searchParams.get("userId");
        const email = searchParams.get("email");
        const displayName = searchParams.get("displayName");
        const errorParam = searchParams.get("error");

        // Check for error first
        if (errorParam) {
          setError(decodeURIComponent(errorParam));
          setLoading(false);
          // Redirect to login page after 3 seconds
          setTimeout(() => {
            navigate("/login", { replace: true });
          }, 3000);
          return;
        }

        // Validate required parameters
        if (!token || !userId) {
          setError("Authentication failed: Missing required information");
          setLoading(false);
          setTimeout(() => {
            navigate("/login", { replace: true });
          }, 3000);
          return;
        }

        // Save authentication token
        setAuthToken(token);

        // Save session information
        saveSession({
          userId,
          email: email || undefined,
          displayName: displayName || undefined
        });

        // Invalidate and refetch user profile to get latest avatar
        if (userId) {
          await queryClient.invalidateQueries({ queryKey: ['userProfile', userId] });
          await queryClient.refetchQueries({ queryKey: ['userProfile', userId] });
        }

        // Process any pending likes from before login
        await processPendingLikes();

        // Redirect to videos page
        navigate("/videos", { replace: true });
      } catch (err) {
        console.error("GitHub callback error:", err);
        setError(err instanceof Error ? err.message : "An unexpected error occurred");
        setLoading(false);
        setTimeout(() => {
          navigate("/login", { replace: true });
        }, 3000);
      }
    };

    handleCallback();
  }, [searchParams, navigate, queryClient]);

  if (loading) {
    return (
      <Box
        sx={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "100vh",
          gap: 2
        }}
      >
        <CircularProgress />
        <Typography variant="body1" color="text.secondary">
          Completing GitHub login...
        </Typography>
      </Box>
    );
  }

  if (error) {
    return (
      <Box
        sx={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "100vh",
          gap: 2,
          px: 2
        }}
      >
        <Alert severity="error" sx={{ maxWidth: 500 }}>
          <Typography variant="h6" gutterBottom>
            GitHub Login Failed
          </Typography>
          <Typography variant="body2">{error}</Typography>
          <Typography variant="body2" sx={{ mt: 1 }}>
            Redirecting to login page...
          </Typography>
        </Alert>
      </Box>
    );
  }

  return null;
};

export default GitHubCallbackPage;

