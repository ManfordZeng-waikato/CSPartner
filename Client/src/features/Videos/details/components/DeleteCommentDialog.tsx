import React from "react";
import { Dialog, DialogTitle, DialogContent, DialogContentText, DialogActions, Button } from "@mui/material";

interface DeleteCommentDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  isPending: boolean;
  isReply?: boolean;
}

const DeleteCommentDialog: React.FC<DeleteCommentDialogProps> = ({
  open,
  onClose,
  onConfirm,
  isPending,
  isReply = false
}) => {
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>{isReply ? 'Delete Reply' : 'Delete Comment'}</DialogTitle>
      <DialogContent>
        <DialogContentText>
          {isReply
            ? 'Are you sure you want to delete this reply? This action cannot be undone.'
            : 'Are you sure you want to delete this comment? All replies to this comment will also be deleted, and this action cannot be undone.'}
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={onConfirm} color="error" variant="contained" disabled={isPending}>
          {isPending ? 'Deleting...' : 'Delete'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default DeleteCommentDialog;

