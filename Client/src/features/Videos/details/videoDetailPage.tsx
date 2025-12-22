import React, { useEffect, useRef } from "react";
import { useParams, useLocation } from "react-router";
import { Box, Divider, CircularProgress, Alert } from "@mui/material";
import { useVideo, useVideoComments } from "../../hooks/useVideos";
import VideoStats from "./components/videoStats";
import VideoComments from "./components/videoComments";
import VideoUploader from "./components/videoUploader";
import VideoInfo from "./components/videoInfo";

const VideoDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const location = useLocation();
  const { video, isLoading: videoLoading, error: videoError } = useVideo(id);
  const { isLoading: commentsLoading } = useVideoComments(id);
  const commentsSectionRef = useRef<HTMLDivElement>(null);

  // Handle scroll to comments section when hash is present
  useEffect(() => {
    // Only scroll if hash is present and both video and comments are loaded
    if (
      location.hash === '#comments' &&
      !videoLoading &&
      !commentsLoading &&
      video &&
      commentsSectionRef.current
    ) {
      const scrollToComments = () => {
        const element = commentsSectionRef.current;
        if (!element) return;

        // Check if element is already visible in viewport
        const rect = element.getBoundingClientRect();
        const isVisible = rect.top >= 0 && rect.top < window.innerHeight * 0.5;

        if (!isVisible) {
          // Use scrollIntoView with block: 'start' for reliable scrolling
          element.scrollIntoView({
            behavior: 'smooth',
            block: 'start',
            inline: 'nearest'
          });
        }
      };

      // Multiple attempts with increasing delays to handle different rendering speeds
      const timeouts: ReturnType<typeof setTimeout>[] = [];

      // Attempt 1: After DOM updates (requestAnimationFrame)
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          scrollToComments();
        });
      });

      // Attempt 2: After a short delay
      timeouts.push(setTimeout(scrollToComments, 200));

      // Attempt 3: After a longer delay (for slow rendering)
      timeouts.push(setTimeout(scrollToComments, 500));

      // Attempt 4: Final attempt after content should be fully loaded
      timeouts.push(setTimeout(scrollToComments, 1000));

      return () => {
        timeouts.forEach(timeout => clearTimeout(timeout));
      };
    }
  }, [location.hash, videoLoading, commentsLoading, video]);

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
        videoId={video.videoId}
        hasLiked={video.hasLiked}
      />

      <Divider sx={{ my: 3 }} />

      {/* Uploader Information */}
      <VideoUploader uploaderUserId={video.uploaderUserId} />

      <Divider sx={{ my: 3 }} />

      {/* Comments Section */}
      <Box id="comments" ref={commentsSectionRef}>
        <VideoComments videoId={id} commentCount={video.commentCount} />
      </Box>
    </Box>
  );
};

export default VideoDetail;

