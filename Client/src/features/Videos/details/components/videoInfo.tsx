import React from "react";
import {
  Box,
  Typography,
  Tooltip
} from "@mui/material";
import { useNavigate } from "react-router";

interface VideoInfoProps {
  title: string;
  description: string | null;
  videoId?: string; // Optional: if provided, title becomes clickable
}

const VideoInfo: React.FC<VideoInfoProps> = ({ title, description, videoId }) => {
  const navigate = useNavigate();

  const handleTitleClick = () => {
    if (videoId) {
      navigate(`/video/${videoId}`);
    }
  };

  const titleElement = videoId ? (
    <Tooltip title="Click to view video details" arrow>
      <Typography 
        variant="h4" 
        component="h1" 
        gutterBottom
        onClick={handleTitleClick}
        sx={{
          cursor: 'pointer',
          display: 'inline-block',
          transition: 'color 0.2s',
          '&:hover': {
            color: 'primary.main'
          }
        }}
      >
        {title}
      </Typography>
    </Tooltip>
  ) : (
    <Typography variant="h4" component="h1" gutterBottom>
      {title}
    </Typography>
  );

  return (
    <Box sx={{ mb: 3 }}>
      {titleElement}
      {description && (
        <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
          {description}
        </Typography>
      )}
    </Box>
  );
};

export default VideoInfo;

