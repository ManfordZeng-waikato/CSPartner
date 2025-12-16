import React from "react";
import {
  Box,
  Typography
} from "@mui/material";

interface VideoInfoProps {
  title: string;
  description: string | null;
}

const VideoInfo: React.FC<VideoInfoProps> = ({ title, description }) => {
  return (
    <Box sx={{ mb: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        {title}
      </Typography>
      {description && (
        <Typography variant="body1" color="text.secondary" sx={{ mb: 2 }}>
          {description}
        </Typography>
      )}
    </Box>
  );
};

export default VideoInfo;

