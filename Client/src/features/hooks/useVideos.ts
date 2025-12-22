import { useMutation, useQuery, useQueryClient, useInfiniteQuery } from "@tanstack/react-query";
import apiClient from "../../lib/api/axios";
import { useAuthSession } from "./useAuthSession";

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

export const useVideos = (pageSize: number = 20) => {
  const { data, isLoading, fetchNextPage, hasNextPage, isFetchingNextPage } = useInfiniteQuery({
    queryKey: ['videos', pageSize],
    queryFn: async ({ pageParam }) => {
      const params = new URLSearchParams();
      params.append('pageSize', pageSize.toString());
      if (pageParam) {
        params.append('cursor', pageParam);
      }
      const response = await apiClient.get<CursorPagedResult<VideoDto>>(`/api/videos?${params.toString()}`);
      return response.data;
    },
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) => lastPage.nextCursor ?? undefined,
  });

  // Flatten all pages into a single array
  const videos = data?.pages.flatMap(page => page.items) ?? [];

  const createVideo = useCreateVideo();

  return { 
    videos, 
    isLoading, 
    createVideo,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage
  };
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

export const useCreateComment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ videoId, content, parentCommentId }: { videoId: string; content: string; parentCommentId?: string | null }): Promise<CommentDto> => {
      const response = await apiClient.post<CommentDto>(`/api/videos/${videoId}/comments`, {
        content,
        parentCommentId: parentCommentId || null
      });
      return response.data;
    },
    onSuccess: async (_, variables) => {
      // Invalidate comments query to refetch
      await queryClient.invalidateQueries({ queryKey: ['video', variables.videoId, 'comments'] });
    }
  });
};

export const useDeleteComment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ commentId, videoId }: { commentId: string; videoId: string }): Promise<void> => {
      await apiClient.delete(`/api/comments/${commentId}`);
    },
    onSuccess: async (_, variables) => {
      // Invalidate comments query to refetch
      // SignalR will also update the comments list, but this ensures consistency
      await queryClient.invalidateQueries({ queryKey: ['video', variables.videoId, 'comments'] });
    }
  });
};

export const useUpdateVideoVisibility = () => {
  const queryClient = useQueryClient();
  const { session } = useAuthSession();

  return useMutation({
    mutationFn: async ({ videoId, visibility }: { videoId: string; visibility: VideoVisibility }): Promise<void> => {
      if (!session?.userId) {
        throw new Error("User not authenticated");
      }
      await apiClient.put(`/api/videos/${videoId}`, {
        title: null,
        description: null,
        thumbnailUrl: null,
        visibility: visibility
      }, {
        params: { userId: session.userId }
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['videos'] });
      await queryClient.invalidateQueries({ queryKey: ['userProfile'] });
    }
  });
};

export const useDeleteVideo = () => {
  const queryClient = useQueryClient();
  const { session } = useAuthSession();

  return useMutation({
    mutationFn: async (videoId: string): Promise<void> => {
      if (!session?.userId) {
        throw new Error("User not authenticated");
      }
      await apiClient.delete(`/api/videos/${videoId}`, {
        params: { userId: session.userId }
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['videos'] });
      await queryClient.invalidateQueries({ queryKey: ['userProfile'] });
    }
  });
};