import React, { useState, useCallback, useMemo } from "react";
import { Box, Typography } from "@mui/material";
import { useVideoComments } from "../../../hooks/useVideos";
import { useAuthSession } from "../../../hooks/useAuthSession";
import { useCommentHub } from "../../../hooks/useCommentHub";
import { useQueryClient } from "@tanstack/react-query";
import CommentForm from "./CommentForm";
import CommentList from "./CommentList";
import { addCommentToList } from "../../../../lib/utils/commentUtils";

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
  // Sort comments by creation time (newest first) to ensure consistent ordering
  const displayComments = useMemo(() => {
    const rawComments = comments.length > 0 ? comments : (initialComments || []);
    return [...rawComments].sort((a, b) => {
      const dateA = new Date(a.createdAtUtc).getTime();
      const dateB = new Date(b.createdAtUtc).getTime();
      return dateB - dateA; // Descending order (newest first)
    });
  }, [comments, initialComments]);

  // Handle real-time comment updates from SignalR (full list replacement)
  const handleCommentsReceived = useCallback((newComments: CommentDto[]) => {
    setComments(newComments);
    // Also update the query cache
    if (videoId) {
      queryClient.setQueryData(['video', videoId, 'comments'], newComments);
    }
  }, [videoId, queryClient]);

  // Handle new comment received (add to existing list - more efficient)
  const handleNewCommentReceived = useCallback((newComment: CommentDto) => {
    setComments(prevComments => {
      const updatedComments = addCommentToList(prevComments, newComment);
      
      // Update query cache
      if (videoId) {
        queryClient.setQueryData(['video', videoId, 'comments'], updatedComments);
      }
      return updatedComments;
    });
  }, [videoId, queryClient]);

  // Connect to SignalR hub for real-time updates
  useCommentHub({
    videoId,
    onCommentsReceived: handleCommentsReceived,
    onNewCommentReceived: handleNewCommentReceived,
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

