import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import apiClient from "../../lib/api/axios";

export const useCreateVideo = (
  onProgress?: (progress: number) => void
) => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (video: CreateVideoDto): Promise<VideoDto> => {
      const formData = new FormData();
      formData.append('VideoFile', video.videoStream);

      if (video.thumbnailStream) {
        formData.append('ThumbnailFile', video.thumbnailStream);
      }

      formData.append('Title', video.title);

      if (video.description !== undefined && video.description !== null) {
        formData.append('Description', video.description);
      }

      formData.append('Visibility', video.visibility.toString());

      const response = await apiClient.post<VideoDto>('/api/videos/upload', formData, {
        onUploadProgress: (progressEvent) => {
          if (progressEvent.total && onProgress) {
            const percentCompleted = Math.round(
              (progressEvent.loaded * 100) / progressEvent.total
            );
            onProgress(percentCompleted);
          }
        }
      });
      return response.data;
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['videos'] });
    }
  });
};

export const useVideos = () => {
  const { data: videos, isLoading } = useQuery({
    queryKey: ['videos'],
    queryFn: async () => {
      const response = await apiClient.get<VideoDto[]>('/api/videos');
      return response.data;
    }
  });

  const createVideo = useCreateVideo();

  return { videos, isLoading, createVideo };
};

export const useVideo = (videoId: string | undefined) => {
  const { data: video, isLoading, error } = useQuery({
    queryKey: ['video', videoId],
    queryFn: async () => {
      if (!videoId) return null
      const response = await apiClient.get<VideoDto>(`/api/videos/${videoId}`)
      return response.data
    },
    enabled: !!videoId
  })

  return { video, isLoading, error }
}

export const useVideoComments = (videoId: string | undefined) => {
  const { data: comments, isLoading } = useQuery({
    queryKey: ['video', videoId, 'comments'],
    queryFn: async () => {
      if (!videoId) return []
      const response = await apiClient.get<CommentDto[]>(`/api/videos/${videoId}/comments`)
      return response.data
    },
    enabled: !!videoId
  })

  return { comments: comments || [], isLoading }
}