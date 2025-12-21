import React from "react";
import { Box, Chip, Tooltip } from "@mui/material";
import type { SxProps, Theme } from "@mui/material";
import VisibilityIcon from "@mui/icons-material/Visibility";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import CommentIcon from "@mui/icons-material/Comment";

interface VideoStatsProps {
  viewCount: number;
  likeCount: number;
  commentCount: number;
  videoId?: string;
  onCommentClick?: () => void;
  sx?: SxProps<Theme>;
}

const VideoStats: React.FC<VideoStatsProps> = ({
  viewCount,
  likeCount,
  commentCount,
  videoId,
  onCommentClick,
  sx
}) => {
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
      <Chip
        icon={<ThumbUpIcon />}
        label={`Likes: ${likeCount}`}
        variant="outlined"
        color="primary"
      />
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

