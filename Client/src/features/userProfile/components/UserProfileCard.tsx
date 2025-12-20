import {
  Box,
  Typography,
  Avatar,
  Paper,
  Chip
} from "@mui/material";
import { getAvatarUrl } from "../../../lib/utils/avatar";
import type { UserProfileDto } from "../../../lib/types";

interface UserProfileCardProps {
  profile: UserProfileDto;
}

function UserProfileCard({ profile }: UserProfileCardProps) {
  return (
    <Paper elevation={3} sx={{ p: 4, mb: 4, borderRadius: 3 }}>
      <Box sx={{ display: "flex", flexDirection: { xs: "column", md: "row" }, gap: 3, alignItems: { xs: "center", md: "flex-start" } }}>
        <Avatar
          src={getAvatarUrl(profile.avatarUrl)}
          sx={{
            width: { xs: 120, md: 150 },
            height: { xs: 120, md: 150 },
            border: 3,
            borderColor: "primary.main"
          }}
        >
          {profile.displayName?.[0] || 'U'}
        </Avatar>

        <Box sx={{ flex: 1, textAlign: { xs: "center", md: "left" } }}>
          <Typography variant="h4" fontWeight={700} gutterBottom>
            {profile.displayName || "Unknown User"}
          </Typography>

          {profile.bio && (
            <Typography variant="body1" color="text.secondary" sx={{ mb: 2, mt: 1 }}>
              {profile.bio}
            </Typography>
          )}

          <Box sx={{ display: "flex", flexWrap: "wrap", gap: 2, justifyContent: { xs: "center", md: "flex-start" }, mt: 2 }}>
            {profile.steamProfileUrl && (
              <Chip
                label="Steam Profile"
                component="a"
                href={profile.steamProfileUrl}
                target="_blank"
                rel="noopener noreferrer"
                clickable
                color="primary"
                variant="outlined"
              />
            )}
            {profile.faceitProfileUrl && (
              <Chip
                label="FACEIT Profile"
                component="a"
                href={profile.faceitProfileUrl}
                target="_blank"
                rel="noopener noreferrer"
                clickable
                color="primary"
                variant="outlined"
              />
            )}
          </Box>

          <Box sx={{ mt: 3, display: "flex", gap: 3, justifyContent: { xs: "center", md: "flex-start" } }}>
            <Box>
              <Typography variant="h6" fontWeight={600}>
                {profile.videos.length}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Highlights
              </Typography>
            </Box>
          </Box>
        </Box>
      </Box>
    </Paper>
  );
}

export default UserProfileCard;

