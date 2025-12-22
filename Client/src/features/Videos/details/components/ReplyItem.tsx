import React, { useState } from "react";
import { Box, Typography, Button, Link, IconButton } from "@mui/material";
import { Reply as ReplyIcon, ArrowForward, Delete as DeleteIcon } from "@mui/icons-material";
import ReplyForm from "./ReplyForm";
import UserNameLink from "./UserNameLink";
import DeleteCommentDialog from "./DeleteCommentDialog";
import { useUserProfile } from "../../../hooks/useUserProfile";
import { useDeleteComment } from "../../../hooks/useVideos";
import { useAuthSession } from "../../../hooks/useAuthSession";
import { formatRelativeTime } from "../../../../lib/utils/date";
import { useNavigate } from "react-router";

interface ReplyItemProps {
  reply: CommentDto;
  videoId: string;
  isAuthenticated: boolean;
  rootCommentUserId: string;
  rootCommentId: string;
}

const ReplyItem: React.FC<ReplyItemProps> = ({
  reply,
  videoId,
  isAuthenticated,
  rootCommentUserId
}) => {
  const [showReplyForm, setShowReplyForm] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const { session } = useAuthSession();
  const deleteComment = useDeleteComment();
  const navigate = useNavigate();
  const isOwner = session?.userId === reply.userId;

  // Determine who this reply is replying to
  // If parentUserId exists, it's replying to that user
  // Otherwise, it's replying to the root comment author
  const repliedToUserId = reply.parentUserId || rootCommentUserId;
  const { profile: repliedToProfile } = useUserProfile(repliedToUserId);
  const repliedToDisplayName = repliedToProfile?.displayName || `User ${repliedToUserId.substring(0, 8)}`;

  const handleRepliedToUserClick = (e: React.MouseEvent) => {
    e.preventDefault();
    navigate(`/user/${repliedToUserId}`);
  };

  const handleDeleteClick = () => {
    setShowDeleteDialog(true);
  };

  const handleDeleteConfirm = async () => {
    try {
      await deleteComment.mutateAsync({ commentId: reply.commentId, videoId });
      setShowDeleteDialog(false);
    } catch (error) {
      console.error('Failed to delete reply:', error);
    }
  };

  return (
    <>
      <Box sx={{ mb: 1.5, pl: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
          <Box sx={{ flex: 1 }}>
            {/* Display format: ReplyAuthor -> RepliedToUser: content */}
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 0.5, flexWrap: 'wrap' }}>
              <UserNameLink userId={reply.userId} size="small" />
              <ArrowForward sx={{ fontSize: '0.75rem', color: 'text.secondary' }} />
              <Link
                component="button"
                onClick={handleRepliedToUserClick}
                sx={{
                  color: 'primary.main',
                  textDecoration: 'none',
                  fontWeight: 500,
                  fontSize: '0.75rem',
                  cursor: 'pointer',
                  '&:hover': {
                    textDecoration: 'underline'
                  }
                }}
              >
                {repliedToDisplayName}
              </Link>
              <Typography variant="caption" sx={{ fontSize: '0.75rem', color: 'text.secondary' }}>
                :
              </Typography>
              <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.7rem', ml: 'auto' }}>
                {formatRelativeTime(reply.createdAtUtc)}
              </Typography>
              {isOwner && (
                <IconButton
                  size="small"
                  onClick={handleDeleteClick}
                  sx={{
                    ml: 0.5,
                    p: 0.25,
                    color: 'error.main',
                    '&:hover': {
                      backgroundColor: 'error.light',
                      color: 'error.dark'
                    }
                  }}
                >
                  <DeleteIcon sx={{ fontSize: '0.875rem' }} />
                </IconButton>
              )}
            </Box>
            <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word', fontSize: '0.875rem' }}>
              {reply.content}
            </Typography>
            {isAuthenticated && (
              <Button
                startIcon={<ReplyIcon />}
                size="small"
                onClick={() => setShowReplyForm(!showReplyForm)}
                sx={{
                  textTransform: 'none',
                  fontSize: '0.75rem',
                  minWidth: 'auto',
                  px: 1,
                  py: 0.25,
                  mt: 0.5
                }}
              >
                Reply
              </Button>
            )}
          </Box>
        </Box>
        {showReplyForm && isAuthenticated && (
          <Box sx={{ mt: 1 }}>
            <ReplyForm
              videoId={videoId}
              parentCommentId={reply.commentId}
              onSuccess={() => setShowReplyForm(false)}
              onCancel={() => setShowReplyForm(false)}
              placeholder="Write a reply..."
            />
          </Box>
        )}
      </Box>
      <DeleteCommentDialog
        open={showDeleteDialog}
        onClose={() => setShowDeleteDialog(false)}
        onConfirm={handleDeleteConfirm}
        isPending={deleteComment.isPending}
        isReply={true}
      />
    </>
  );
};

export default ReplyItem;

