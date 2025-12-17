import React from "react";
import { useParams } from "react-router";
import { Box, Divider, CircularProgress, Alert } from "@mui/material";
import { useVideo } from "../../hooks/useVideos";
import VideoStats from "./components/videoStats";
import VideoComments from "./components/videoComments";
import VideoUploader from "./components/videoUploader";
import VideoInfo from "./components/videoInfo";

const VideoDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { video, isLoading: videoLoading, error: videoError } = useVideo(id);

  if (videoLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  if (videoError || !video) {
    return (
      <Box p={3}>
        <Alert severity="error">Video not found or failed to load</Alert>
      </Box>
    );
  }

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', p: 3 }}>
      {/* Video Player */}
      <Box sx={{ mb: 3 }}>
        <video
          controls
          width="100%"
          style={{ maxHeight: '600px', borderRadius: '8px' }}
          src={video.videoUrl}
          preload="metadata"
        >
          Your browser does not support video playback.
        </video>
      </Box>

      {/* Video Title and Description */}
      <VideoInfo title={video.title} description={video.description} />

      {/* Statistics */}
      <VideoStats
        viewCount={video.viewCount}
        likeCount={video.likeCount}
        commentCount={video.commentCount}
      />

      <Divider sx={{ my: 3 }} />

      {/* Uploader Information */}
      <VideoUploader uploaderUserId={video.uploaderUserId} />

      <Divider sx={{ my: 3 }} />

      {/* Comments Section */}
      <VideoComments videoId={id} commentCount={video.commentCount} />
    </Box>
  );
};

export default VideoDetail;

