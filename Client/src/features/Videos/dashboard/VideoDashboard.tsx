import { Box } from "@mui/material"
import VideoCard from "../videoCard";
import { useVideos } from "../../hooks/useVideos";
import { useInfiniteScroll } from "../../hooks/useInfiniteScroll";

export default function VideoDashboard() {
  const { videos, fetchNextPage, hasNextPage, isFetchingNextPage } = useVideos();
  const observerTarget = useInfiniteScroll({
    hasNextPage: hasNextPage ?? false,
    fetchNextPage,
    isFetchingNextPage: isFetchingNextPage ?? false,
  });

  return (
    <Box>
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
        {videos.map(video => (
          <VideoCard key={video.videoId} video={video} />
        ))}
      </Box>
      
      {/* Infinite scroll trigger element */}
      {hasNextPage && (
        <Box
          ref={observerTarget}
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            padding: 4,
          }}
        />
      )}
    </Box>
  )
}
