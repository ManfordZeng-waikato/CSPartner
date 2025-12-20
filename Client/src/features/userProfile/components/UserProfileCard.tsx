import {
  Box,
  Typography,
  Avatar,
  Paper,
  Chip,
  IconButton,
  Tooltip
} from "@mui/material";
import { Edit } from "@mui/icons-material";
import { useNavigate } from "react-router";
import { getAvatarUrl } from "../../../lib/utils/avatar";

interface UserProfileCardProps {
  profile: UserProfileDto;
  visibleVideoCount?: number;
  isOwnProfile?: boolean;
}

function UserProfileCard({ profile, visibleVideoCount, isOwnProfile = false }: UserProfileCardProps) {
  const navigate = useNavigate();

  const handleEditClick = () => {
    navigate(`/user/${profile.userId}/edit`);
  };

  return (
    <Paper elevation={3} sx={{ p: 4, mb: 4, borderRadius: 3, position: "relative" }}>
      {isOwnProfile && (
        <Tooltip title="Click to edit profile" arrow>
          <IconButton
            onClick={handleEditClick}
            sx={{
              position: "absolute",
              top: 16,
              right: 16,
              backgroundColor: "primary.main",
              color: "white",
              "&:hover": {
                backgroundColor: "primary.dark"
              }
            }}
          >
            <Edit />
          </IconButton>
        </Tooltip>
      )}
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
          <Box sx={{ display: "flex", alignItems: "center", gap: 2, justifyContent: { xs: "center", md: "flex-start" } }}>
            <Typography variant="h4" fontWeight={700} gutterBottom>
              {profile.displayName || "Unknown User"}
            </Typography>
          </Box>

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
                {visibleVideoCount ?? profile.videos.length}
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

