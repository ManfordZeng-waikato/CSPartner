    import { 
    Card, 
    CardContent, 
    Box,
    Chip
    } from "@mui/material"
    import VisibilityIcon from "@mui/icons-material/Visibility"
    import ThumbUpIcon from "@mui/icons-material/ThumbUp"
    import CommentIcon from "@mui/icons-material/Comment"
    import VideoInfo from "./details/components/videoInfo"

    interface VideoCardProps {
    video: VideoDto;
    }

    export default function VideoCard({ video }: VideoCardProps) {
    return (
        <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        <CardContent sx={{ flexGrow: 1 }}>
            <VideoInfo title={video.title} description={video.description} />
            <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mt: 2 }}>
            <Chip 
                icon={<VisibilityIcon />}
                label={video.viewCount} 
                size="small" 
                variant="outlined"
            />
            <Chip 
                icon={<ThumbUpIcon />}
                label={video.likeCount} 
                size="small" 
                variant="outlined"
            />
            <Chip 
                icon={<CommentIcon />}
                label={video.commentCount} 
                size="small" 
                variant="outlined"
            />
            </Box>
            <Box sx={{ mt: 2 }}>
            <video
                controls
                width="100%"
                style={{ maxHeight: '500px', minHeight: '400px' }}
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
