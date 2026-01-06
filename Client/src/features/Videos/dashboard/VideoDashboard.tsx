import { Box } from "@mui/material"
import VideoCard from "../VideoCard";
import VideoCardSkeleton from "../VideoCardSkeleton";
import { useVideos } from "../../hooks/useVideos";
import { useInfiniteScroll } from "../../hooks/useInfiniteScroll";

export default function VideoDashboard() {
  const { videos, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading } = useVideos();
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
        {/* Show skeleton screens during initial load */}
        {isLoading ? (
          <>
            <VideoCardSkeleton />
            <VideoCardSkeleton />
            <VideoCardSkeleton />
            <VideoCardSkeleton />
          </>
        ) : (
          videos.map(video => (
            <VideoCard key={video.videoId} video={video} />
          ))
        )}
        
        {/* Show skeleton screens while fetching next page */}
        {isFetchingNextPage && (
          <>
            <VideoCardSkeleton />
            <VideoCardSkeleton />
          </>
        )}
      </Box>
      
      {/* Infinite scroll trigger element */}
      {hasNextPage && !isFetchingNextPage && (
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
