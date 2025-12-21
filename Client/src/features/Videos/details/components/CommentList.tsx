import React from "react";
import { Box, Typography, Paper, CircularProgress } from "@mui/material";
import CommentItem from "./CommentItem";
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

interface CommentListProps {
  comments: CommentDto[];
  isLoading: boolean;
}

const CommentList: React.FC<CommentListProps> = ({ comments, isLoading }) => {
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
        <CommentItem key={comment.commentId} comment={comment} />
      ))}
    </Box>
  );
};

export default CommentList;

