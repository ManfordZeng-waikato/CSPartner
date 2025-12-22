import React from "react";
import {
  Box,
  Typography,
  Avatar,
  CircularProgress,
  Tooltip
} from "@mui/material";
import { useNavigate } from "react-router";
import { useUserProfile } from "../../../hooks/useUserProfile";
import { getAvatarUrl } from "../../../../lib/utils/avatar";

interface VideoUploaderProps {
  uploaderUserId: string | undefined;
}

const VideoUploader: React.FC<VideoUploaderProps> = ({ uploaderUserId }) => {
  const { profile, isLoading: profileLoading } = useUserProfile(uploaderUserId);
  const navigate = useNavigate();

  const handleClick = () => {
    if (uploaderUserId) {
      navigate(`/user/${uploaderUserId}`);
    }
  };

  const tooltipTitle = profile?.displayName 
    ? `${profile.displayName} - Click to view profile`
    : "Click to view profile";

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
            <Tooltip title={tooltipTitle} arrow>
              <Avatar
                src={getAvatarUrl(profile?.avatarUrl)}
                onClick={handleClick}
                sx={{ 
                  width: 56, 
                  height: 56,
                  cursor: 'pointer',
                  transition: 'transform 0.2s',
                  '&:hover': {
                    transform: 'scale(1.05)'
                  }
                }}
              >
                {profile?.displayName?.[0] || 'U'}
              </Avatar>
            </Tooltip>
            <Box>
              <Tooltip title={tooltipTitle} arrow>
                <Typography 
                  variant="subtitle1" 
                  fontWeight="bold"
                  onClick={handleClick}
                  sx={{
                    cursor: 'pointer',
                    display: 'inline-block',
                    transition: 'color 0.2s',
                    '&:hover': {
                      color: 'primary.main'
                    }
                  }}
                >
                  {profile?.displayName || 'Unknown User'}
                </Typography>
              </Tooltip>
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

