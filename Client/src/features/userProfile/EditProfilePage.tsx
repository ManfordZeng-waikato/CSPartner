import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router";
import {
  Container,
  Paper,
  Button,
  Box,
  Typography,
  Avatar,
  Grid,
  CircularProgress
} from "@mui/material";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useUserProfile, useUpdateUserProfile } from "../hooks/useUserProfile";
import { useAuthSession } from "../hooks/useAuthSession";
import { updateProfileSchema, type UpdateProfileFormValues } from "../../lib/schemas/userProfileSchema";
import { AVAILABLE_AVATARS } from "../../lib/constants/avatars";
import RequireAuth from "../../app/shared/components/RequireAuth";
import { FormTextField } from "../../app/shared/components";
import { handleApiError } from "../hooks/useAccount";

function EditProfilePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { session } = useAuthSession();
  const { profile, isLoading: profileLoading } = useUserProfile(id);
  const updateProfile = useUpdateUserProfile();

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    control,
    formState: { errors }
  } = useForm<UpdateProfileFormValues>({
    resolver: zodResolver(updateProfileSchema),
    defaultValues: {
      displayName: "",
      bio: "",
      avatarUrl: AVAILABLE_AVATARS[0],
      steamProfileUrl: "",
      faceitProfileUrl: ""
    }
  });

  const selectedAvatar = useWatch({ control, name: "avatarUrl" });
  const [serverError, setServerError] = useState<string | null>(null);

  // Check if user is viewing their own profile
  const isOwnProfile = session?.userId === id;

  // Load profile data into form - use reset to avoid cascading renders
  useEffect(() => {
    if (profile) {
      reset({
        displayName: profile.displayName || "",
        bio: profile.bio || "",
        avatarUrl: profile.avatarUrl || AVAILABLE_AVATARS[0],
        steamProfileUrl: profile.steamProfileUrl || "",
        faceitProfileUrl: profile.faceitProfileUrl || ""
      });
    }
  }, [profile, reset]);

  const onSubmit = async (values: UpdateProfileFormValues) => {
    if (!id || !isOwnProfile) {
      setServerError("You can only edit your own profile");
      return;
    }

    setServerError(null);

    try {
      await updateProfile.mutateAsync({
        userId: id,
        data: {
          displayName: values.displayName || null,
          bio: values.bio || null,
          avatarUrl: values.avatarUrl || null,
          steamProfileUrl: values.steamProfileUrl || null,
          faceitProfileUrl: values.faceitProfileUrl || null
        }
      });
      navigate(`/user/${id}`);
    } catch (error: unknown) {
      setServerError(handleApiError(error));
    }
  };

  if (!isOwnProfile) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Paper sx={{ p: 4, textAlign: "center" }}>
          <Typography variant="h5" color="error">
            You can only edit your own profile
          </Typography>
          <Button variant="contained" sx={{ mt: 2 }} onClick={() => navigate(-1)}>
            Go Back
          </Button>
        </Paper>
      </Container>
    );
  }

  if (profileLoading) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "400px" }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  return (
    <RequireAuth>
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Paper elevation={3} sx={{ p: 4, borderRadius: 3 }}>
          <Typography variant="h4" fontWeight={700} gutterBottom>
            Edit Profile
          </Typography>

          {serverError && (
            <Box sx={{ mb: 3, p: 2, bgcolor: "error.light", color: "error.contrastText", borderRadius: 1 }}>
              {serverError}
            </Box>
          )}

          <form onSubmit={handleSubmit(onSubmit)}>
            <Box sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
              {/* Avatar Selection */}
              <Box>
                <Typography variant="body2" sx={{ mb: 1, fontWeight: 500 }}>
                  Choose your avatar
                </Typography>
                <Grid container spacing={2}>
                  {AVAILABLE_AVATARS.map((avatar) => (
                    <Grid  key={avatar}>
                      <Box
                        onClick={() => setValue("avatarUrl", avatar)}
                        sx={{
                          cursor: "pointer",
                          border: selectedAvatar === avatar ? 3 : 1,
                          borderColor: selectedAvatar === avatar ? "primary.main" : "divider",
                          borderRadius: 2,
                          p: 1,
                          display: "flex",
                          justifyContent: "center",
                          alignItems: "center",
                          transition: "all 0.2s",
                          "&:hover": {
                            borderColor: "primary.main",
                            transform: "scale(1.05)"
                          }
                        }}
                      >
                        <Avatar
                          src={avatar}
                          sx={{
                            width: 80,
                            height: 80
                          }}
                        />
                      </Box>
                    </Grid>
                  ))}
                </Grid>
              </Box>

              {/* Display Name */}
              <FormTextField
                label="Display Name"
                fullWidth
                register={register("displayName")}
                error={errors.displayName}
                disabled={updateProfile.isPending}
              />

              {/* Bio */}
              <FormTextField
                label="Bio"
                fullWidth
                multiline
                rows={4}
                register={register("bio")}
                error={errors.bio}
                helperText="Maximum 500 characters"
                disabled={updateProfile.isPending}
              />

              {/* Steam Profile URL */}
              <FormTextField
                label="Steam Profile URL"
                fullWidth
                placeholder="https://steamcommunity.com/profiles/..."
                register={register("steamProfileUrl")}
                error={errors.steamProfileUrl}
                disabled={updateProfile.isPending}
              />

              {/* FACEIT Profile URL */}
              <FormTextField
                label="FACEIT Profile URL"
                fullWidth
                placeholder="https://www.faceit.com/en/players/..."
                register={register("faceitProfileUrl")}
                error={errors.faceitProfileUrl}
                disabled={updateProfile.isPending}
              />

              {/* Action Buttons */}
              <Box sx={{ display: "flex", gap: 2, justifyContent: "flex-end", mt: 2 }}>
                <Button
                  variant="outlined"
                  onClick={() => navigate(`/user/${id}`)}
                  disabled={updateProfile.isPending}
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  variant="contained"
                  disabled={updateProfile.isPending}
                >
                  {updateProfile.isPending ? "Saving..." : "Save Changes"}
                </Button>
              </Box>
            </Box>
          </form>
        </Paper>
      </Container>
    </RequireAuth>
  );
}

export default EditProfilePage;

