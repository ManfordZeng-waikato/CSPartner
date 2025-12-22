import React from "react";
import { Box, Typography, Avatar, Tooltip } from "@mui/material";
import { useNavigate } from "react-router";
import { useUserProfile } from "../../../hooks/useUserProfile";
import { getAvatarUrl } from "../../../../lib/utils/avatar";

interface CommentAuthorProps {
  userId: string;
  size?: 'small' | 'medium';
}

const CommentAuthor: React.FC<CommentAuthorProps> = ({ userId, size = 'medium' }) => {
  const { profile, isLoading } = useUserProfile(userId);
  const navigate = useNavigate();
  const avatarSize = size === 'small' ? 24 : 32;
  const displayName = profile?.displayName || `User ${userId.substring(0, 8)}`;
  const avatarUrl = getAvatarUrl(profile?.avatarUrl);

  const handleClick = () => {
    if (userId) {
      navigate(`/user/${userId}`);
    }
  };

  const tooltipTitle = profile?.displayName 
    ? `${profile.displayName} - Click to view profile`
    : "Click to view profile";

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
      <Tooltip title={tooltipTitle} arrow>
        <Avatar 
          src={avatarUrl}
          onClick={handleClick}
          sx={{ 
            width: avatarSize, 
            height: avatarSize,
            cursor: 'pointer',
            transition: 'transform 0.2s',
            '&:hover': {
              transform: 'scale(1.1)'
            }
          }}
        >
          {displayName.substring(0, 2).toUpperCase()}
        </Avatar>
      </Tooltip>
      <Tooltip title={tooltipTitle} arrow>
        <Typography 
          variant={size === 'small' ? 'caption' : 'body2'} 
          onClick={handleClick}
          sx={{ 
            fontWeight: 500,
            cursor: 'pointer',
            display: 'inline-block',
            transition: 'color 0.2s',
            '&:hover': {
              color: 'primary.main'
            }
          }}
        >
          {displayName}
        </Typography>
      </Tooltip>
    </Box>
  );
};

export default CommentAuthor;

