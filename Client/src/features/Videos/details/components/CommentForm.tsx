import React, { useState } from "react";
import { Paper, TextField, Button, Alert, Link as MuiLink, Typography } from "@mui/material";
import { Link, useLocation } from "react-router";
import { useCreateComment } from "../../../hooks/useVideos";
import { handleApiError } from "../../../hooks/useAccount";

interface CommentFormProps {
  videoId: string;
  isAuthenticated: boolean;
}

const CommentForm: React.FC<CommentFormProps> = ({ videoId, isAuthenticated }) => {
  const createComment = useCreateComment();
  const location = useLocation();
  const [commentContent, setCommentContent] = useState("");
  const [error, setError] = useState<string | null>(null);
  
  // Build return URL with current path and comments anchor
  const returnUrl = `${location.pathname}${location.search}#comments`;

  const handleSubmit = async (e: React.FormEvent) => {
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
      // SignalR will automatically update comments via ReceiveComments event
    } catch (err: unknown) {
      setError(handleApiError(err));
    }
  };

  if (!isAuthenticated) {
    return (
      <Paper sx={{ p: 2, mb: 3, bgcolor: 'action.hover' }}>
        <Typography variant="body2" color="text.secondary" align="center">
          Please{" "}
          <Link 
            to="/login" 
            state={{ from: { pathname: returnUrl } }}
            style={{ textDecoration: 'none', color: 'inherit' }}
          >
            <MuiLink component="span" underline="hover">
              log in
            </MuiLink>
          </Link>
          {" "}to post a comment
        </Typography>
      </Paper>
    );
  }

  return (
    <Paper sx={{ p: 2, mb: 3 }}>
      <form onSubmit={handleSubmit}>
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
  );
};

export default CommentForm;

