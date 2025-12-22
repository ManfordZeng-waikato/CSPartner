import React, { useState } from "react";
import { Box, TextField, Button, Alert, IconButton } from "@mui/material";
import { Send, Close } from "@mui/icons-material";
import { useCreateComment, useVideoComments } from "../../../hooks/useVideos";
import { useQueryClient } from "@tanstack/react-query";

interface ReplyFormProps {
  videoId: string;
  parentCommentId: string;
  onSuccess?: () => void;
  onCancel?: () => void;
  placeholder?: string;
}

const ReplyForm: React.FC<ReplyFormProps> = ({ 
  videoId, 
  parentCommentId, 
  onSuccess,
  onCancel,
  placeholder = "Write a reply..."
}) => {
  const createComment = useCreateComment();
  const queryClient = useQueryClient();
  const [replyContent, setReplyContent] = useState("");
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!videoId || !replyContent.trim()) return;

    setError(null);
    try {
      await createComment.mutateAsync({
        videoId,
        content: replyContent.trim(),
        parentCommentId: parentCommentId
      });
      
      // Force refresh comments list after a short delay to ensure SignalR update is processed
      // If SignalR doesn't update, this will ensure the comment appears
      setTimeout(() => {
        queryClient.invalidateQueries({ queryKey: ['video', videoId, 'comments'] });
      }, 500);
      
      setReplyContent("");
      if (onSuccess) {
        onSuccess();
      }
      // SignalR will automatically update comments via ReceiveComments event
    } catch (err: unknown) {
      console.error('Failed to create reply:', err);
      let errorMessage = "Failed to post reply. Please try again.";
      
      if (err && typeof err === 'object') {
        const axiosError = err as { response?: { data?: { error?: string } }; message?: string };
        errorMessage = axiosError.response?.data?.error || axiosError.message || errorMessage;
      }
      
      setError(errorMessage);
    }
  };

  const handleCancel = () => {
    setReplyContent("");
    setError(null);
    if (onCancel) {
      onCancel();
    }
  };

  return (
    <Box sx={{ mt: 1.5 }}>
      <form onSubmit={handleSubmit}>
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'flex-start' }}>
          <TextField
            fullWidth
            multiline
            rows={2}
            placeholder={placeholder}
            value={replyContent}
            onChange={(e) => setReplyContent(e.target.value)}
            disabled={createComment.isPending}
            size="small"
            sx={{ flex: 1 }}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
                e.preventDefault();
                handleSubmit(e as any);
              }
            }}
          />
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
            <IconButton
              type="submit"
              color="primary"
              disabled={!replyContent.trim() || createComment.isPending}
              size="small"
              sx={{ minWidth: 40 }}
            >
              <Send fontSize="small" />
            </IconButton>
            {onCancel && (
              <IconButton
                type="button"
                onClick={handleCancel}
                size="small"
                sx={{ minWidth: 40 }}
              >
                <Close fontSize="small" />
              </IconButton>
            )}
          </Box>
        </Box>
        {error && (
          <Alert severity="error" sx={{ mt: 1 }}>
            {error}
          </Alert>
        )}
      </form>
    </Box>
  );
};

export default ReplyForm;

