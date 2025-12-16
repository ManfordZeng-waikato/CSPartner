import React from "react";
import {
  Box,
  Typography,
  Avatar,
  CircularProgress
} from "@mui/material";
import { useUserProfile } from "../../../hooks/useUserProfile";

interface VideoUploaderProps {
  uploaderUserId: string | undefined;
}

const VideoUploader: React.FC<VideoUploaderProps> = ({ uploaderUserId }) => {
  const { profile, isLoading: profileLoading } = useUserProfile(uploaderUserId);

  return (
    <Box sx={{ mb: 3 }}>
      <Typography variant="h6" gutterBottom>
        Uploader
      </Typography>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mt: 2 }}>
        {profileLoading ? (
          <CircularProgress size={24} />
        ) : (
          <>
            <Avatar
              src={profile?.avatarUrl || undefined}
              sx={{ width: 56, height: 56 }}
            >
              {profile?.displayName?.[0] || 'U'}
            </Avatar>
            <Box>
              <Typography variant="subtitle1" fontWeight="bold">
                {profile?.displayName || 'Unknown User'}
              </Typography>
              {profile?.bio && (
                <Typography variant="body2" color="text.secondary">
                  {profile.bio}
                </Typography>
              )}
            </Box>
          </>
        )}
      </Box>
    </Box>
  );
};

export default VideoUploader;

