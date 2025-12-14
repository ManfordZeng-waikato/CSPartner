import { 
  Typography, 
  Container, 
  Card, 
  CardContent, 
  Box,
  Chip
} from "@mui/material"
import VisibilityIcon from "@mui/icons-material/Visibility"
import ThumbUpIcon from "@mui/icons-material/ThumbUp"
import CommentIcon from "@mui/icons-material/Comment"
import axios from "axios";
import { useEffect, useState } from "react";

function App() {
 const[videos, setVideos] = useState<VideoDto[]>([]);

 useEffect(() => {
 axios.get<VideoDto[]>('/api/videos')
  .then(response => setVideos(response.data))
  .catch(error => console.error('Error fetching videos:', error));
 }, []);
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Typography variant="h3" gutterBottom sx={{ mb: 4 }}>
        CSPartner
      </Typography>
      <Box 
        sx={{ 
          display: 'grid', 
          gridTemplateColumns: { 
            xs: '1fr', 
            sm: 'repeat(2, 1fr)', 
            md: 'repeat(3, 1fr)' 
          }, 
          gap: 3 
        }}
      >
        {videos.map(video => (
          <Card key={video.videoId} sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            <CardContent sx={{ flexGrow: 1 }}>
              <Typography variant="h6" component="h2" gutterBottom noWrap>
                {video.title}
              </Typography>
              {video.description && (
                <Typography 
                  variant="body2" 
                  color="text.secondary" 
                  sx={{ 
                    mb: 2,
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    display: '-webkit-box',
                    WebkitLineClamp: 2,
                    WebkitBoxOrient: 'vertical'
                  }}
                >
                  {video.description}
                </Typography>
              )}
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
                  style={{ maxHeight: '300px' }}
                  src={video.videoUrl}
                  preload="metadata"
                >
                  Your browser does not support video playback.
                </video>
              </Box>
            </CardContent>
          </Card>
        ))}
      </Box>
    </Container>
  )
}

export default App
