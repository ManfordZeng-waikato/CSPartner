import React from "react";
import { Box, Typography, Avatar } from "@mui/material";
import { useUserProfile } from "../../../hooks/useUserProfile";
import { getAvatarUrl } from "../../../../lib/utils/avatar";

interface CommentAuthorProps {
  userId: string;
  size?: 'small' | 'medium';
}

const CommentAuthor: React.FC<CommentAuthorProps> = ({ userId, size = 'medium' }) => {
  const { profile, isLoading } = useUserProfile(userId);
  const avatarSize = size === 'small' ? 24 : 32;
  const displayName = profile?.displayName || `User ${userId.substring(0, 8)}`;
  const avatarUrl = getAvatarUrl(profile?.avatarUrl);

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
        <Avatar sx={{ width: avatarSize, height: avatarSize }} />
        <Typography variant="caption" color="text.secondary">
          Loading...
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Avatar 
        src={avatarUrl}
        sx={{ width: avatarSize, height: avatarSize }}
      >
        {displayName.substring(0, 2).toUpperCase()}
      </Avatar>
      <Typography 
        variant={size === 'small' ? 'caption' : 'body2'} 
        sx={{ fontWeight: 500 }}
      >
        {displayName}
      </Typography>
    </Box>
  );
};

export default CommentAuthor;

