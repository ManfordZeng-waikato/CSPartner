import React, { useState } from "react";
import { useNavigate } from "react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useCreateVideo } from "../../hooks/useVideos";
import { videoUploadSchema } from "../../../lib/schemas/videoUploadSchema";
import type { VideoUploadFormValues } from "../../../lib/schemas/videoUploadSchema";
import VideoEditorForm from "../../../app/shared/components/VideoEditorForm";

const visibilityOptions: { label: string; value: VideoVisibility }[] = [
  { label: "Public", value: 1 as VideoVisibility },
  { label: "Private", value: 2 as VideoVisibility }
];

const VideoUploadForm: React.FC = () => {
  const createVideo = useCreateVideo();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);
  const [isSuccess, setIsSuccess] = useState(false);

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
      visibility: visibilityOptions[0].value,
      uploaderUserId: ""
    }
  });

  const resetToEmpty = () =>
    reset({
      title: "",
      description: "",
      videoFile: null,
      visibility: visibilityOptions[0].value,
      uploaderUserId: ""
    });

  const resetAfterSuccess = (uploaderUserId: string) =>
    reset({
      title: "",
      description: "",
      videoFile: null,
      visibility: visibilityOptions[0].value,
      uploaderUserId
    });

  const onSubmit = async (values: VideoUploadFormValues) => {
    setServerError(null);
    setIsSuccess(false);

    const description = values.description?.trim();
    const payload: CreateVideoDto = {
      videoStream: values.videoFile as File,
      videoFileName: (values.videoFile as File).name,
      thumbnailStream: null,
      thumbnailFileName: null,
      title: values.title.trim(),
      description: description ? description : null,
      visibility: values.visibility as VideoVisibility,
      uploaderUserId: values.uploaderUserId.trim()
    };

    try {
      const createdVideo = await createVideo.mutateAsync(payload);
      setIsSuccess(true);
      resetAfterSuccess(values.uploaderUserId.trim());
      if (createdVideo?.videoId) {
        navigate(`/video/${createdVideo.videoId}`);
      }
    } catch (error: unknown) {
      let message = "Upload failed, please try again later";
      if (
        error &&
        typeof error === "object" &&
        "response" in error &&
        (error as { response?: { data?: { message?: string } } }).response?.data
          ?.message
      ) {
        message = (error as { response: { data: { message: string } } }).response
          .data.message;
      } else if (error instanceof Error && error.message) {
        message = error.message;
      }
      setServerError(message);
    }
  };

  return (
    <VideoEditorForm
      title="Upload Video"
      subtitle="Select a video, add details and visibility. Submit to call the upload API."
      control={control}
      register={register}
      handleSubmit={handleSubmit}
      errors={errors}
      isPending={createVideo.isPending}
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

