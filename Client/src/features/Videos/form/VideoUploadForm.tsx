import React, { useState } from "react";
import { useNavigate } from "react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useCreateVideo } from "../../hooks/useVideos";
import { videoUploadSchema } from "../../../lib/schemas/videoUploadSchema";
import type { VideoUploadFormValues } from "../../../lib/schemas/videoUploadSchema";
import VideoEditorForm from "../../../app/shared/components/VideoEditorForm";
import { useAuthSession } from "../../hooks/useAuthSession";
import { getAuthToken } from "../../../lib/api/axios";

const visibilityOptions: { label: string; value: VideoVisibility }[] = [
  { label: "Public", value: 1 as VideoVisibility },
  { label: "Private", value: 2 as VideoVisibility }
];

const VideoUploadForm: React.FC = () => {
  const navigate = useNavigate();
  const { session } = useAuthSession();
  const [serverError, setServerError] = useState<string | null>(null);
  const [isSuccess, setIsSuccess] = useState(false);
  const [uploadProgress, setUploadProgress] = useState<number>(0);
  
  const createVideo = useCreateVideo((progress) => {
    setUploadProgress(progress);
  });

  const {
    control,
    handleSubmit,
    register,
    reset,
    formState: { errors }
  } = useForm<VideoUploadFormValues>({mode: "onTouched",
    resolver: zodResolver(videoUploadSchema),
    defaultValues: {
      title: "",
      description: "",
      videoFile: null,
      visibility: visibilityOptions[0].value
    }
  });

  const resetToEmpty = () =>
    reset({
      title: "",
      description: "",
      videoFile: null,
      visibility: visibilityOptions[0].value
    });

  const onSubmit = async (values: VideoUploadFormValues) => {
    setServerError(null);
    setIsSuccess(false);

    // Check authentication before submitting
    const token = getAuthToken();
    if (!token || !session) {
      setServerError("Please log in to upload videos");
      navigate("/login");
      return;
    }

    const description = values.description?.trim();
    const payload: CreateVideoDto = {
      videoStream: values.videoFile as File,
      videoFileName: (values.videoFile as File).name,
      thumbnailStream: null,
      thumbnailFileName: null,
      title: values.title.trim(),
      description: description ? description : null,
      visibility: values.visibility as VideoVisibility
    };

    try {
      setUploadProgress(0);
      const createdVideo = await createVideo.mutateAsync(payload);
      setIsSuccess(true);
      setUploadProgress(100);
      resetToEmpty();
      if (createdVideo?.videoId) {
        navigate(`/video/${createdVideo.videoId}`);
      }
    } catch (error: unknown) {
      setUploadProgress(0);
      console.error("Video upload error:", error);
      let message = "Upload failed, please try again later";
      
      // Handle axios error response
      if (
        error &&
        typeof error === "object" &&
        "response" in error
      ) {
        const axiosError = error as { 
          response?: { 
            data?: { 
              message?: string;
              error?: string;
              errors?: string[] | string;
            };
            status?: number;
          };
          message?: string;
        };
        
        if (axiosError.response?.data) {
          const data = axiosError.response.data;
          // Try different error message formats
          if (data.error) {
            message = typeof data.error === "string" ? data.error : String(data.error);
          } else if (data.message) {
            message = data.message;
          } else if (data.errors) {
            if (Array.isArray(data.errors) && data.errors.length > 0) {
              message = data.errors[0];
            } else if (typeof data.errors === "string") {
              message = data.errors;
            }
          } else if (axiosError.response.status === 401) {
            message = "Authentication failed. Please log in again.";
          } else if (axiosError.response.status === 403) {
            message = "You don't have permission to perform this action.";
          }
        } else if (axiosError.message) {
          message = axiosError.message;
        }
      } else if (error instanceof Error) {
        message = error.message || message;
      }
      
      setServerError(message);
    }
  };

  return (
    <VideoEditorForm
      title="Upload Video"
      subtitle="Upload your video to the platform. You can choose to make it public or private."
      control={control}
      register={register}
      handleSubmit={handleSubmit}
      errors={errors}
      isPending={createVideo.isPending}
      uploadProgress={uploadProgress}
      serverError={serverError}
      isSuccess={isSuccess}
      visibilityOptions={visibilityOptions}
      onSubmit={onSubmit}
      onReset={resetToEmpty}
      pendingLabel="Uploading..."
      submitLabel="Submit"
    />
  );
};

export default VideoUploadForm;

