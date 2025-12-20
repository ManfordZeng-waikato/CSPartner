import {
    Card,
    CardContent,
    Box,
    Avatar,
    Tooltip,
    Chip
} from "@mui/material"
import { Lock, Public } from "@mui/icons-material"
import { useNavigate } from "react-router"
import VideoInfo from "./details/components/videoInfo"
import { useUserProfile } from "../hooks/useUserProfile"
import { useAuthSession } from "../hooks/useAuthSession"
import VideoStats from "./details/components/videoStats"
import { getAvatarUrl } from "../../lib/utils/avatar"

interface VideoCardProps {
    video: VideoDto;
}

export default function VideoCard({ video }: VideoCardProps) {
    const { profile } = useUserProfile(video.uploaderUserId);
    const { session } = useAuthSession();
    const navigate = useNavigate();

    // 判断是否是视频所有者
    const isOwner = session?.userId === video.uploaderUserId;

    const handleAvatarClick = () => {
        if (video.uploaderUserId) {
            navigate(`/user/${video.uploaderUserId}`);
        }
    };

    const tooltipTitle = profile?.displayName 
        ? `${profile.displayName} - Click to view profile`
        : "Click to view profile";

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
            {/* 可见性标签 - 仅对视频所有者显示，位于头像下方 */}
            {isOwner && (
                <Chip
                    icon={video.visibility === 1 ? <Public sx={{ fontSize: 16 }} /> : <Lock sx={{ fontSize: 16 }} />}
                    label={video.visibility === 1 ? "Public" : "Private"}
                    size="small"
                    color={video.visibility === 1 ? "success" : "default"}
                    sx={{
                        position: 'absolute',
                        top: 64, // 头像高度(40) + 顶部间距(16) + 标签与头像间距(8)
                        right: 16, // 与头像右对齐
                        zIndex: 1,
                        fontWeight: 500
                    }}
                />
            )}
            <CardContent sx={{ flexGrow: 1 }}>
                <VideoInfo title={video.title} description={video.description} />
                <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 2 }}>
                    <VideoStats
                        viewCount={video.viewCount}
                        likeCount={video.likeCount}
                        commentCount={video.commentCount}
                    />
                </Box>
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
