import React, { useState } from "react";
import { Box, Typography, Paper, Button, IconButton } from "@mui/material";
import { Reply as ReplyIcon, Delete as DeleteIcon } from "@mui/icons-material";
import CommentAuthor from "./CommentAuthor";
import ReplyForm from "./ReplyForm";
import ReplyItem from "./ReplyItem";
import DeleteCommentDialog from "./DeleteCommentDialog";
import { useDeleteComment } from "../../../hooks/useVideos";
import { useAuthSession } from "../../../hooks/useAuthSession";
import { formatRelativeTime } from "../../../../lib/utils/date";

interface CommentItemProps {
  comment: CommentDto;
  videoId: string;
  isAuthenticated: boolean;
}

const CommentItem: React.FC<CommentItemProps> = ({ comment, videoId, isAuthenticated }) => {
  const [showReplyForm, setShowReplyForm] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const { session } = useAuthSession();
  const deleteComment = useDeleteComment();
  const isOwner = session?.userId === comment.userId;

  const handleReplyClick = () => {
    setShowReplyForm(!showReplyForm);
  };

  const handleReplySuccess = () => {
    setShowReplyForm(false);
  };

  const handleDeleteClick = () => {
    setShowDeleteDialog(true);
  };

  const handleDeleteConfirm = async () => {
    try {
      await deleteComment.mutateAsync({ commentId: comment.commentId, videoId });
      setShowDeleteDialog(false);
    } catch (error) {
      console.error('Failed to delete comment:', error);
    }
  };

  return (
    <>
      <Paper key={comment.commentId} sx={{ p: 2, mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 1 }}>
          <Box sx={{ flex: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <CommentAuthor userId={comment.userId} size="medium" />
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Typography variant="caption" color="text.secondary">
                  {formatRelativeTime(comment.createdAtUtc)}
                </Typography>
                {isOwner && (
                  <IconButton
                    size="small"
                    onClick={handleDeleteClick}
                    sx={{
                      p: 0.5,
                      color: 'error.main',
                      '&:hover': {
                        backgroundColor: 'error.light',
                        color: 'error.dark'
                      }
                    }}
                  >
                    <DeleteIcon sx={{ fontSize: '1rem' }} />
                  </IconButton>
                )}
              </Box>
            </Box>
            <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
              {comment.content}
            </Typography>
            {isAuthenticated && (
              <Button
                startIcon={<ReplyIcon />}
                size="small"
                onClick={handleReplyClick}
                sx={{ mt: 1, textTransform: 'none' }}
              >
                Reply
              </Button>
            )}
            {showReplyForm && isAuthenticated && (
              <ReplyForm
                videoId={videoId}
                parentCommentId={comment.commentId}
                onSuccess={handleReplySuccess}
                onCancel={() => setShowReplyForm(false)}
              />
            )}
          </Box>
        </Box>
        {comment.replies && comment.replies.length > 0 && (
          <Box sx={{ mt: 2 }}>
            {comment.replies.map((reply) => (
              <ReplyItem
                key={reply.commentId}
                reply={reply}
                videoId={videoId}
                isAuthenticated={isAuthenticated}
                rootCommentUserId={comment.userId}
                rootCommentId={comment.commentId}
              />
            ))}
          </Box>
        )}
      </Paper>
      <DeleteCommentDialog
        open={showDeleteDialog}
        onClose={() => setShowDeleteDialog(false)}
        onConfirm={handleDeleteConfirm}
        isPending={deleteComment.isPending}
        isReply={false}
      />
    </>
  );
};

export default CommentItem;

