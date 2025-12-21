import React from "react";
import { Box, Typography, Paper } from "@mui/material";
import CommentAuthor from "./CommentAuthor";
import { formatRelativeTime } from "../../../../lib/utils/date";

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

interface CommentItemProps {
  comment: CommentDto;
}

const CommentItem: React.FC<CommentItemProps> = ({ comment }) => {
  return (
    <Paper key={comment.commentId} sx={{ p: 2, mb: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
        <Box sx={{ flex: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
            <CommentAuthor userId={comment.userId} size="medium" />
            <Typography variant="caption" color="text.secondary">
              {formatRelativeTime(comment.createdAtUtc)}
            </Typography>
          </Box>
          <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
            {comment.content}
          </Typography>
        </Box>
      </Box>
      {comment.replies && comment.replies.length > 0 && (
        <Box sx={{ mt: 2, ml: 5, pl: 2, borderLeft: '2px solid', borderColor: 'divider' }}>
          {comment.replies.map((reply) => (
            <Paper key={reply.commentId} sx={{ p: 1.5, mb: 1, bgcolor: 'action.hover' }}>
              <Box sx={{ display: 'flex', alignItems: 'flex-start' }}>
                <Box sx={{ flex: 1 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 0.5 }}>
                    <CommentAuthor userId={reply.userId} size="small" />
                    <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.7rem' }}>
                      {formatRelativeTime(reply.createdAtUtc)}
                    </Typography>
                  </Box>
                  <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
                    {reply.content}
                  </Typography>
                </Box>
              </Box>
            </Paper>
          ))}
        </Box>
      )}
    </Paper>
  );
};

export default CommentItem;

