import React from "react";
import { Box, Chip, Tooltip } from "@mui/material";
import type { SxProps, Theme } from "@mui/material";
import VisibilityIcon from "@mui/icons-material/Visibility";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import ThumbUpOutlinedIcon from "@mui/icons-material/ThumbUpOutlined";
import CommentIcon from "@mui/icons-material/Comment";
import { useToggleLike } from "../../../hooks/useVideos";
import { useAuthSession } from "../../../hooks/useAuthSession";

interface VideoStatsProps {
  viewCount: number;
  likeCount: number;
  commentCount: number;
  videoId?: string;
  hasLiked?: boolean;
  onCommentClick?: () => void;
  sx?: SxProps<Theme>;
}

const VideoStats: React.FC<VideoStatsProps> = ({
  viewCount,
  likeCount,
  commentCount,
  videoId,
  hasLiked = false,
  onCommentClick,
  sx
}) => {
  const toggleLike = useToggleLike();
  const { session } = useAuthSession();
  const isAuthenticated = !!session;

  const handleLikeClick = async () => {
    if (!videoId) return;
    
    try {
      await toggleLike.mutateAsync(videoId);
    } catch (error: unknown) {
      // Error handling is done in the hook (redirects to login if not authenticated)
      // Silently ignore throttle errors and login redirect errors
      const errorMessage = error instanceof Error ? error.message : String(error);
      if (errorMessage !== "Please login to like videos" && 
          errorMessage !== "Please wait before liking again") {
        console.error("Failed to toggle like:", error);
      }
    }
  };

  const likeChip = (
    <Chip
      icon={hasLiked ? <ThumbUpIcon /> : <ThumbUpOutlinedIcon />}
      label={`Likes: ${likeCount}`}
      variant={hasLiked ? "filled" : "outlined"}
      color={hasLiked ? "primary" : "primary"}
      onClick={videoId ? handleLikeClick : undefined}
      disabled={toggleLike.isPending}
      sx={{
        cursor: videoId ? 'pointer' : 'default',
        transition: 'all 0.2s',
        '&:hover': videoId ? {
          backgroundColor: hasLiked ? 'primary.dark' : 'action.hover',
          transform: 'scale(1.05)'
        } : {},
        opacity: toggleLike.isPending ? 0.6 : 1
      }}
    />
  );

  const commentChip = (
    <Chip
      icon={<CommentIcon />}
      label={`Comments: ${commentCount}`}
      variant="outlined"
      color="primary"
      onClick={onCommentClick}
      sx={{
        cursor: onCommentClick ? 'pointer' : 'default',
        transition: 'all 0.2s',
        '&:hover': onCommentClick ? {
          backgroundColor: 'action.hover',
          transform: 'scale(1.05)'
        } : {}
      }}
    />
  );

  return (
    <Box
      sx={{
        display: "flex",
        gap: 2,
        alignItems: "center",
        mb: 3,
        flexWrap: "wrap",
        ...sx
      }}
    >
      <Chip
        icon={<VisibilityIcon />}
        label={`Views: ${viewCount}`}
        variant="outlined"
        color="primary"
      />
      {videoId ? (
        <Tooltip 
          title={isAuthenticated ? (hasLiked ? "Unlike" : "Like") : "Login to like"} 
          arrow
        >
          {likeChip}
        </Tooltip>
      ) : (
        likeChip
      )}
      {onCommentClick ? (
        <Tooltip title="Click to view comments" arrow>
          {commentChip}
        </Tooltip>
      ) : (
        commentChip
      )}
    </Box>
  );
};

export default VideoStats;

