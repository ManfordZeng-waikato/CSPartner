import React, { useState, useCallback } from "react";
import { Box, Typography } from "@mui/material";
import { useVideoComments } from "../../../hooks/useVideos";
import { useAuthSession } from "../../../hooks/useAuthSession";
import { useCommentHub } from "../../../hooks/useCommentHub";
import { useQueryClient } from "@tanstack/react-query";
import CommentForm from "./CommentForm";
import CommentList from "./CommentList";

// CommentDto type definition
interface CommentDto {
  commentId: string;
  videoId: string;
  userId: string;
  parentCommentId: string | null;
  content: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  replies: CommentDto[];
}

interface VideoCommentsProps {
  videoId: string | undefined;
  commentCount: number;
}

const VideoComments: React.FC<VideoCommentsProps> = ({ videoId, commentCount }) => {
  const queryClient = useQueryClient();
  const { comments: initialComments, isLoading: commentsLoading } = useVideoComments(videoId);
  const { session } = useAuthSession();
  const [comments, setComments] = useState<CommentDto[]>([]);

  const isAuthenticated = !!session;

  // Use initialComments as the source of truth, but allow SignalR to override
  const displayComments = comments.length > 0 ? comments : (initialComments || []);

  // Handle real-time comment updates from SignalR
  const handleCommentsReceived = useCallback((newComments: CommentDto[]) => {
    setComments(newComments);
    // Also update the query cache
    if (videoId) {
      queryClient.setQueryData(['video', videoId, 'comments'], newComments);
    }
  }, [videoId, queryClient]);

  // Connect to SignalR hub for real-time updates
  useCommentHub({
    videoId,
    onCommentsReceived: handleCommentsReceived,
    enabled: !!videoId
  });

  // Calculate actual comment count (including replies)
  const totalCommentCount = displayComments.reduce((count, comment) => {
    return count + 1 + (comment.replies?.length || 0);
  }, 0);

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Comments ({totalCommentCount > 0 ? totalCommentCount : commentCount})
      </Typography>

      {videoId && (
        <CommentForm videoId={videoId} isAuthenticated={isAuthenticated} />
      )}

      <CommentList comments={displayComments} isLoading={commentsLoading} />
    </Box>
  );
};

export default VideoComments;

