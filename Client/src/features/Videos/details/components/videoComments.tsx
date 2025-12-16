import React from "react";
import {
  Box,
  Typography,
  Paper,
  CircularProgress
} from "@mui/material";
import { useVideoComments } from "../../../hooks/useVideos";

interface VideoCommentsProps {
  videoId: string | undefined;
  commentCount: number;
}

const VideoComments: React.FC<VideoCommentsProps> = ({ videoId, commentCount }) => {
  const { comments, isLoading: commentsLoading } = useVideoComments(videoId);

  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Comments ({commentCount})
      </Typography>
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

