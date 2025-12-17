import React from "react";
import { Box, Chip } from "@mui/material";
import type { SxProps, Theme } from "@mui/material";
import VisibilityIcon from "@mui/icons-material/Visibility";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import CommentIcon from "@mui/icons-material/Comment";

interface VideoStatsProps {
  viewCount: number;
  likeCount: number;
  commentCount: number;
  sx?: SxProps<Theme>;
}

const VideoStats: React.FC<VideoStatsProps> = ({
  viewCount,
  likeCount,
  commentCount,
  sx
}) => (
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
    <Chip
      icon={<CommentIcon />}
      label={`Comments: ${commentCount}`}
      variant="outlined"
      color="primary"
    />
  </Box>
);

export default VideoStats;

