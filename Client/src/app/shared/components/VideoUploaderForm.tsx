import React from "react";
import {
  Alert,
  Box,
  Button,
  FormControl,
  FormHelperText,
  InputLabel,
  LinearProgress,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography
} from "@mui/material";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import {
  Controller,
  type Control,
  type FieldErrors,
  type UseFormHandleSubmit,
  type UseFormRegister
} from "react-hook-form";
import type { VideoUploadFormValues } from "../../../lib/schemas/videoUploadSchema";
import { MAP_OPTIONS, WEAPON_OPTIONS } from "../../../lib/schemas/videoUploadSchema";

interface VideoEditorFormProps {
  title?: string;
  subtitle?: string;
  control: Control<VideoUploadFormValues>;
  register: UseFormRegister<VideoUploadFormValues>;
  handleSubmit: UseFormHandleSubmit<VideoUploadFormValues>;
  errors: FieldErrors<VideoUploadFormValues>;
  isPending?: boolean;
  uploadProgress?: number;
  serverError?: string | null;
  isSuccess?: boolean;
  visibilityOptions: { label: string; value: VideoVisibility }[];
  onSubmit: (values: VideoUploadFormValues) => void | Promise<void>;
  onReset: () => void;
  submitLabel?: string;
  pendingLabel?: string;
  hideFileInput?: boolean;
}

const VideoEditorForm: React.FC<VideoEditorFormProps> = ({
  title = "Upload Video",
  subtitle = "Select a video, add details and visibility. Submit to call the upload API.",
  control,
  register,
  handleSubmit,
  errors,
  isPending = false,
  uploadProgress,
  serverError = null,
  isSuccess = false,
  visibilityOptions,
  onSubmit,
  onReset,
  submitLabel = "Submit",
  pendingLabel = "Uploading...",
  hideFileInput = false
}) => {
  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h5" fontWeight={700} gutterBottom>
        {title}
      </Typography>
      {subtitle && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {subtitle}
        </Typography>
      )}

      {isPending && (
        <Box sx={{ mb: 2 }}>
          <LinearProgress
            variant={uploadProgress !== undefined ? "determinate" : "indeterminate"}
            value={uploadProgress}
            sx={{ mb: 1 }}
          />
          {uploadProgress !== undefined && (
            <Typography variant="body2" color="text.secondary" align="center">
              Uploading: {uploadProgress}%
            </Typography>
          )}
        </Box>
      )}
      {serverError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {serverError}
        </Alert>
      )}
      {isSuccess && (
        <Alert severity="success" sx={{ mb: 2 }}>
          Upload succeeded! The video was submitted.
        </Alert>
      )}

      <Box component="form" noValidate onSubmit={handleSubmit(onSubmit)}>
        <Stack spacing={2}>
          <TextField
            label="Title"
            fullWidth
            {...register("title")}
            id="video-title"
            name="title"
            error={!!errors.title}
            helperText={errors.title?.message || "Up to 10 words"}
            disabled={isPending}
          />

          <TextField
            label="Description (optional)"
            fullWidth
            multiline
            minRows={3}
            {...register("description")}
            id="video-description"
            name="description"
            error={!!errors.description}
            helperText={errors.description?.message || "Up to 20 words"}
            disabled={isPending}
          />

          {!hideFileInput && (
            <Controller
              name="videoFile"
              control={control}
              render={({ field }) => (
                <Box>
                  <input
                    id="video-file-input"
                    name="videoFile"
                    type="file"
                    accept="video/*"
                    style={{ display: 'none' }}
                    onChange={(event) => {
                      const file = event.target.files?.[0] ?? null;
                      field.onChange(file);
                      // Reset input to allow selecting the same file again
                      event.target.value = "";
                    }}
                  />
                  <label htmlFor="video-file-input">
                    <Button
                      component="span"
                      variant="contained"
                      startIcon={<CloudUploadIcon />}
                      disabled={isPending}
                    >
                      Choose video file
                    </Button>
                  </label>
                  {field.value && (
                    <Typography variant="body2" sx={{ mt: 1 }}>
                      Selected: {(field.value as File).name}
                    </Typography>
                  )}
                  {errors.videoFile ? (
                    <Typography
                      variant="caption"
                      color="error"
                      display="block"
                      sx={{ mt: 0.5 }}
                    >
                      {errors.videoFile.message?.toString()}
                    </Typography>
                  ) : (
                    <Typography
                      variant="caption"
                      color="text.secondary"
                      display="block"
                      sx={{ mt: 0.5 }}
                    >
                      Max file size 50 MB
                    </Typography>
                  )}
                </Box>
              )}
            />
          )}

          <Controller
            name="map"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth error={!!errors.map} disabled={isPending}>
                <InputLabel id="video-map-label">Map *</InputLabel>
                <Select
                  labelId="video-map-label"
                  id="video-map"
                  label="Map *"
                  value={field.value || ""}
                  onChange={(e) => field.onChange(e.target.value)}
                  inputProps={{ id: "video-map-input" }}
                >
                  {MAP_OPTIONS.map((map) => (
                    <MenuItem key={map} value={map}>
                      {map}
                    </MenuItem>
                  ))}
                </Select>
                <FormHelperText>{errors.map?.message || "Select the map where this highlight was recorded"}</FormHelperText>
              </FormControl>
            )}
          />

          <Controller
            name="weapon"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth error={!!errors.weapon} disabled={isPending}>
                <InputLabel id="video-weapon-label">Weapon Type *</InputLabel>
                <Select
                  labelId="video-weapon-label"
                  id="video-weapon"
                  label="Weapon Type *"
                  value={field.value || ""}
                  onChange={(e) => field.onChange(e.target.value)}
                  inputProps={{ id: "video-weapon-input" }}
                >
                  {WEAPON_OPTIONS.map((weapon) => (
                    <MenuItem key={weapon} value={weapon}>
                      {weapon}
                    </MenuItem>
                  ))}
                </Select>
                <FormHelperText>{errors.weapon?.message || "Select the weapon type used in this highlight"}</FormHelperText>
              </FormControl>
            )}
          />

          <Controller
            name="visibility"
            control={control}
            render={({ field }) => (
              <FormControl fullWidth error={!!errors.visibility} disabled={isPending}>
                <InputLabel id="video-visibility-label">Visibility</InputLabel>

                <Select
                  labelId="video-visibility-label"
                  id="video-visibility"
                  label="Visibility"
                  value={field.value}
                  onChange={(e) => field.onChange(Number(e.target.value))}
                  inputProps={{ id: "video-visibility-input" }} 
                >
                  {visibilityOptions.map((option) => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </Select>

                <FormHelperText>{errors.visibility?.message}</FormHelperText>
              </FormControl>
            )}
          />


          <Stack direction="row" spacing={2} justifyContent="flex-end" sx={{ pt: 1 }}>
            <Button variant="outlined" onClick={onReset} disabled={isPending}>
              Reset
            </Button>
            <Button type="submit" variant="contained" disabled={isPending}>
              {isPending ? pendingLabel : submitLabel}
            </Button>
          </Stack>
        </Stack>
      </Box>
    </Paper>
  );
};

export default VideoEditorForm;

