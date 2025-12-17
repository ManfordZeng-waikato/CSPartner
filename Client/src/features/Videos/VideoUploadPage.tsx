import React from "react";
import { Box } from "@mui/material";
import VideoUploadForm from "./form/VideoUploadForm";

const VideoUploadPage: React.FC = () => {
  return (
    <Box sx={{ maxWidth: 900, mx: "auto" }}>
      <VideoUploadForm />
    </Box>
  );
};

export default VideoUploadPage;

