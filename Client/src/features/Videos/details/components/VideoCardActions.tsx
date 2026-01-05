import {
  Box,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button
} from "@mui/material";
import { Lock, Public, Delete } from "@mui/icons-material";
import { isVideoPublic } from "../../../../lib/utils/videoVisibility";

// Visibility label component (absolute positioned)
export function VideoVisibilityLabel({ video, isOwner }: { video: VideoDto; isOwner: boolean }) {
  if (!isOwner) return null;

  const isPublic = isVideoPublic(video.visibility);

  return (
    <Chip
      icon={isPublic ? <Public sx={{ fontSize: 16 }} /> : <Lock sx={{ fontSize: 16 }} />}
      label={isPublic ? "Public" : "Private"}
      size="small"
      color={isPublic ? "success" : "default"}
      sx={{
        position: 'absolute',
        top: 64, // Avatar height(40) + top spacing(16) + label-avatar spacing(8)
        right: 16, // Aligned with avatar right
        zIndex: 1,
        fontWeight: 500
      }}
    />
  );
}

// Delete confirmation dialog component
export function VideoDeleteDialog({ 
  isOwner, 
  showMenu, 
  deleteDialogOpen, 
  onDeleteConfirm, 
  onDeleteCancel, 
  isDeleting 
}: { 
  video: VideoDto; 
  isOwner: boolean; 
  showMenu: boolean;
  deleteDialogOpen: boolean;
  onDeleteConfirm: () => void;
  onDeleteCancel: () => void;
  isDeleting: boolean;
}) {
  if (!showMenu || !isOwner) return null;

  return (
    <Dialog
      open={deleteDialogOpen}
      onClose={onDeleteCancel}
    >
      <DialogTitle>Delete Video</DialogTitle>
      <DialogContent>
        <DialogContentText>
          Are you sure you want to delete this video? This action cannot be undone.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onDeleteCancel}>Cancel</Button>
        <Button onClick={onDeleteConfirm} color="error" variant="contained" disabled={isDeleting}>
          {isDeleting ? "Deleting..." : "Delete"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

// Action buttons component (rendered in CardContent)
export function VideoActionButtons({ 
  video, 
  isOwner, 
  showMenu, 
  onToggleVisibility, 
  onDeleteClick, 
  isUpdating, 
  isDeleting 
}: { 
  video: VideoDto; 
  isOwner: boolean; 
  showMenu: boolean;
  onToggleVisibility: () => void;
  onDeleteClick: () => void;
  isUpdating: boolean;
  isDeleting: boolean;
}) {
  if (!showMenu || !isOwner) return null;

  const isPublic = isVideoPublic(video.visibility);

  return (
    <Box sx={{ display: 'flex', gap: 1, mt: 2, flexWrap: 'wrap' }}>
      <Button
        variant="outlined"
        size="small"
        startIcon={isPublic ? <Lock /> : <Public />}
        onClick={onToggleVisibility}
        disabled={isUpdating}
        color={isPublic ? "secondary" : "success"}
      >
        {isUpdating 
          ? "Updating..." 
          : isPublic 
            ? "Make Private" 
            : "Make Public"}
      </Button>
      <Button
        variant="outlined"
        size="small"
        startIcon={<Delete />}
        onClick={onDeleteClick}
        disabled={isDeleting}
        color="error"
      >
        Delete
      </Button>
    </Box>
  );
}


