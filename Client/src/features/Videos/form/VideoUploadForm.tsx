import React, { useState } from "react";
import { useNavigate } from "react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useCreateVideo } from "../../hooks/useVideos";
import { videoUploadSchema } from "../../../lib/schemas/videoUploadSchema";
import type { VideoUploadFormValues } from "../../../lib/schemas/videoUploadSchema";
import VideoUploaderForm from "../../../app/shared/components/VideoUploaderForm";
import { useAuthSession } from "../../hooks/useAuthSession";
import { getAuthToken } from "../../../lib/api/axios";
import { handleApiError } from "../../hooks/useAccount";

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
      videoFile: values.videoFile as File,
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
      setServerError(handleApiError(error));
    }
  };

  return (
    <VideoUploaderForm
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

