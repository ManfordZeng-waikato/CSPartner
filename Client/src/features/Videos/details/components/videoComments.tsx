import React, { useState } from "react";
import {
  Box,
  Typography,
  Paper,
  CircularProgress,
  TextField,
  Button,
  Alert,
  Link as MuiLink
} from "@mui/material";
import { useVideoComments, useCreateComment } from "../../../hooks/useVideos";
import { useAuthSession } from "../../../hooks/useAuthSession";

interface VideoCommentsProps {
  videoId: string | undefined;
  commentCount: number;
}

const VideoComments: React.FC<VideoCommentsProps> = ({ videoId, commentCount }) => {
  const { comments, isLoading: commentsLoading } = useVideoComments(videoId);
  const { session } = useAuthSession();
  const createComment = useCreateComment();
  const [commentContent, setCommentContent] = useState("");
  const [error, setError] = useState<string | null>(null);

  const isAuthenticated = !!session;

  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!videoId || !commentContent.trim()) return;

    setError(null);
    try {
      await createComment.mutateAsync({
        videoId,
        content: commentContent.trim(),
        parentCommentId: null
      });
      setCommentContent("");
    } catch (err: any) {
      setError(err.response?.data?.error || "Failed to post comment. Please try again.");
    }
  };

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Comments ({commentCount})
      </Typography>

      {/* Comment Form - Only show for authenticated users */}
      {isAuthenticated ? (
        <Paper sx={{ p: 2, mb: 3 }}>
          <form onSubmit={handleSubmitComment}>
            <TextField
              fullWidth
              multiline
              rows={3}
              placeholder="Write a comment..."
              value={commentContent}
              onChange={(e) => setCommentContent(e.target.value)}
              disabled={createComment.isPending}
              sx={{ mb: 2 }}
            />
            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}
            <Button
              type="submit"
              variant="contained"
              disabled={!commentContent.trim() || createComment.isPending}
            >
              {createComment.isPending ? "Posting..." : "Post Comment"}
            </Button>
          </form>
        </Paper>
      ) : (
        <Paper sx={{ p: 2, mb: 3, bgcolor: 'action.hover' }}>
          <Typography variant="body2" color="text.secondary" align="center">
            Please{" "}
            <MuiLink href="/login" underline="hover">
              log in
            </MuiLink>
            {" "}to post a comment
          </Typography>
        </Paper>
      )}

      {/* Comments List */}
      {commentsLoading ? (
        <Box display="flex" justifyContent="center" p={3}>
          <CircularProgress />
        </Box>
      ) : comments.length === 0 ? (
        <Paper sx={{ p: 3, mt: 2, textAlign: 'center' }}>
          <Typography variant="body2" color="text.secondary">
            No comments yet
          </Typography>
        </Paper>
      ) : (
        <Box sx={{ mt: 2 }}>
          {comments.map((comment) => (
            <Paper key={comment.commentId} sx={{ p: 2, mb: 2 }}>
              <Typography variant="body1" sx={{ mb: 1 }}>
                {comment.content}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {new Date(comment.createdAtUtc).toLocaleString('en-US')}
              </Typography>
              {comment.replies && comment.replies.length > 0 && (
                <Box sx={{ mt: 2, ml: 3, pl: 2, borderLeft: '2px solid', borderColor: 'divider' }}>
                  {comment.replies.map((reply) => (
                    <Paper key={reply.commentId} sx={{ p: 1.5, mb: 1, bgcolor: '#FFF3E0' }}>
                      <Typography variant="body2" sx={{ mb: 0.5 }}>
                        {reply.content}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {new Date(reply.createdAtUtc).toLocaleString('en-US')}
                      </Typography>
                    </Paper>
                  ))}
                </Box>
              )}
            </Paper>
          ))}
        </Box>
      )}
    </Box>
  );
};

export default VideoComments;

