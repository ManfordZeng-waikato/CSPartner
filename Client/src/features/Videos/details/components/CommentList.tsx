import React from "react";
import { Box, Typography, Paper, CircularProgress } from "@mui/material";
import CommentItem from "./CommentItem";

interface CommentListProps {
  comments: CommentDto[];
  isLoading: boolean;
  videoId: string;
  isAuthenticated: boolean;
}

const CommentList: React.FC<CommentListProps> = ({ comments, isLoading, videoId, isAuthenticated }) => {
  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" p={3}>
        <CircularProgress />
      </Box>
    );
  }

  if (comments.length === 0) {
    return (
      <Paper sx={{ p: 3, mt: 2, textAlign: 'center' }}>
        <Typography variant="body2" color="text.secondary">
          No comments yet
        </Typography>
      </Paper>
    );
  }

  return (
    <Box sx={{ mt: 2 }}>
      {comments.map((comment) => (
        <CommentItem 
          key={comment.commentId} 
          comment={comment} 
          videoId={videoId}
          isAuthenticated={isAuthenticated}
        />
      ))}
    </Box>
  );
};

export default CommentList;

