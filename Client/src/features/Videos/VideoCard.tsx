import { useState } from "react";
import {
    Card,
    CardContent,
    Box,
    Avatar,
    Tooltip
} from "@mui/material"
import { useNavigate } from "react-router"
import VideoInfo from "./details/components/videoInfo"
import { useUserProfile } from "../hooks/useUserProfile"
import { useAuthSession } from "../hooks/useAuthSession"
import { useUpdateVideoVisibility, useDeleteVideo } from "../hooks/useVideos"
import VideoStats from "./details/components/videoStats"
import { VideoVisibilityLabel, VideoDeleteDialog, VideoActionButtons } from "./details/components/VideoCardActions"
import { getAvatarUrl } from "../../lib/utils/avatar"

interface VideoCardProps {
    video: VideoDto;
    showMenu?: boolean; // Whether to show the three-dot menu
}

export default function VideoCard({ video, showMenu = false }: VideoCardProps) {
    const { profile } = useUserProfile(video.uploaderUserId);
    const { session } = useAuthSession();
    const navigate = useNavigate();
    const updateVisibility = useUpdateVideoVisibility();
    const deleteVideo = useDeleteVideo();
    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

    // Check if current user is the video owner
    const isOwner = session?.userId === video.uploaderUserId;

    const handleAvatarClick = () => {
        if (video.uploaderUserId) {
            navigate(`/user/${video.uploaderUserId}`);
        }
    };

    const tooltipTitle = profile?.displayName 
        ? `${profile.displayName} - Click to view profile`
        : "Click to view profile";

    const handleToggleVisibility = async () => {
        const newVisibility = video.visibility === 1 ? 2 : 1;
        try {
            await updateVisibility.mutateAsync({ videoId: video.videoId, visibility: newVisibility });
        } catch (error) {
            console.error("Failed to update video visibility:", error);
        }
    };

    const handleDeleteClick = () => {
        setDeleteDialogOpen(true);
    };

    const handleDeleteConfirm = async () => {
        try {
            await deleteVideo.mutateAsync(video.videoId);
            setDeleteDialogOpen(false);
        } catch (error) {
            console.error("Failed to delete video:", error);
        }
    };

    const handleDeleteCancel = () => {
        setDeleteDialogOpen(false);
    };

    const handleCommentClick = () => {
        navigate(`/video/${video.videoId}#comments`);
    };

    return (
        <Card elevation={3} sx={{ height: '100%', display: 'flex', flexDirection: 'column', position: 'relative', borderRadius: 3, overflow: 'hidden' }}>
            <Tooltip title={tooltipTitle} arrow>
                <Avatar
                    src={getAvatarUrl(profile?.avatarUrl)}
                    onClick={handleAvatarClick}
                    sx={{
                        position: 'absolute',
                        top: 16,
                        right: 16,
                        width: 40,
                        height: 40,
                        zIndex: 1,
                        cursor: 'pointer',
                        transition: 'transform 0.2s',
                        '&:hover': {
                            transform: 'scale(1.1)'
                        }
                    }}
                >
                    {profile?.displayName?.[0] || 'U'}
                </Avatar>
            </Tooltip>
            <VideoVisibilityLabel video={video} isOwner={isOwner} />
            <VideoDeleteDialog
                video={video}
                isOwner={isOwner}
                showMenu={showMenu}
                deleteDialogOpen={deleteDialogOpen}
                onDeleteConfirm={handleDeleteConfirm}
                onDeleteCancel={handleDeleteCancel}
                isDeleting={deleteVideo.isPending}
            />
            <CardContent sx={{ flexGrow: 1 }}>
                <VideoInfo title={video.title} description={video.description} videoId={video.videoId} />
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 2 }}>
                    <VideoStats
                        viewCount={video.viewCount}
                        likeCount={video.likeCount}
                        commentCount={video.commentCount}
                        videoId={video.videoId}
                        hasLiked={video.hasLiked}
                        onCommentClick={handleCommentClick}
                    />
                </Box>
                <VideoActionButtons
                    video={video}
                    isOwner={isOwner}
                    showMenu={showMenu}
                    onToggleVisibility={handleToggleVisibility}
                    onDeleteClick={handleDeleteClick}
                    isUpdating={updateVisibility.isPending}
                    isDeleting={deleteVideo.isPending}
                />
                <Box sx={{ mt: 2 }}>
                    <video
                        controls
                        width="100%"
                        style={{ maxHeight: '500px', minHeight: '400px', borderRadius: '8px' }}
                        src={video.videoUrl}
                        preload="metadata"
                    >
                        Your browser does not support video playback.
                    </video>
                </Box>
            </CardContent>
        </Card>
    );
}
