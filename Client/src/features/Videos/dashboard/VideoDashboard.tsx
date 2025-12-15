import { Box, Typography } from "@mui/material"
import VideoCard from "../VideoCard";
import { useVideos } from "../../hooks/useVideos";

export default function VideoDashboard() {
 
const { videos, isLoading } = useVideos();
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: '1fr',
          sm: '1fr',
          md: 'repeat(2, 1fr)'
        },
        gap: 3
      }}
    >
      {isLoading || !videos ? (<Typography variant="h6" component="h2" gutterBottom noWrap>Loading...</Typography>) :
       (
        videos.map(video => (
          <VideoCard key={video.videoId} video={video} />
        ))
      )}
    </Box>
  )
}
