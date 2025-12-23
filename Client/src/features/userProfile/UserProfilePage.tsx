
import { useParams } from "react-router";
import {
  Box,
  Container,
  Typography,
  Paper,
  CircularProgress
} from "@mui/material";
import { useUserProfile } from "../hooks/useUserProfile";
import { useAuthSession } from "../hooks/useAuthSession";
import VideoCard from "../Videos/VideoCard";
import UserProfileCard from "./components/UserProfileCard";

function UserProfilePage() {
  const { id } = useParams<{ id: string }>();
  const { profile, isLoading } = useUserProfile(id);
  const { session } = useAuthSession();
  
  // Check if viewing own profile
  const isViewingOwnProfile = session?.userId === id;
  
  const visibleVideos = profile?.videos.filter(video => {
    if (isViewingOwnProfile) {
     
      return true;
    }
    // Viewing others' profiles, only show Public videos
    return video.visibility === 1; // VideoVisibility.Public = 1
  }) ?? [];

  if (isLoading) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "400px" }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!profile) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <Paper sx={{ p: 4, textAlign: "center" }}>
          <Typography variant="h5" color="error">
            User not found
          </Typography>
        </Paper>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <UserProfileCard 
        profile={profile} 
        visibleVideoCount={visibleVideos.length} 
        isOwnProfile={isViewingOwnProfile}
      />

      {/* Video list */}
      <Box>
        {visibleVideos.length === 0 ? (
          <Paper sx={{ p: 4, textAlign: "center" }}>
            <Typography variant="body1" color="text.secondary">
              {profile.videos.length === 0 
                ? "No videos uploaded yet"
                : isViewingOwnProfile 
                  ? "No videos to display"
                  : "No public videos available"}
            </Typography>
          </Paper>
        ) : (
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
            {visibleVideos.map(video => (
              <VideoCard key={video.videoId} video={video} showMenu={isViewingOwnProfile} />
            ))}
          </Box>
        )}
      </Box>
    </Container>
  );
}

export default UserProfilePage;
