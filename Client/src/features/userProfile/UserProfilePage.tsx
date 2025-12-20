
import { useParams } from "react-router";
import {
  Box,
  Container,
  Typography,
  Paper,
  CircularProgress
} from "@mui/material";
import { useUserProfile } from "../hooks/useUserProfile";
import VideoCard from "../Videos/videoCard";
import UserProfileCard from "./components/UserProfileCard";

function UserProfilePage() {
  const { id } = useParams<{ id: string }>();
  const { profile, isLoading } = useUserProfile(id);

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
      <UserProfileCard profile={profile} />

      {/* 视频列表 */}
      <Box>
        {profile.videos.length === 0 ? (
          <Paper sx={{ p: 4, textAlign: "center" }}>
            <Typography variant="body1" color="text.secondary">
              No videos uploaded yet
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
            {profile.videos.map(video => (
              <VideoCard key={video.videoId} video={video} />
            ))}
          </Box>
        )}
      </Box>
    </Container>
  );
}

export default UserProfilePage;
