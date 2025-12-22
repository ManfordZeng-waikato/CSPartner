import { useMutation, useQuery, useQueryClient, useInfiniteQuery } from "@tanstack/react-query";
import { useRef } from "react";
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

// Store pending like actions for unauthenticated users
const PENDING_LIKES_KEY = 'pending_likes';

const getPendingLikes = (): string[] => {
  if (typeof window === 'undefined') return [];
  const stored = localStorage.getItem(PENDING_LIKES_KEY);
  return stored ? JSON.parse(stored) : [];
};

const addPendingLike = (videoId: string) => {
  if (typeof window === 'undefined') return;
  const pending = getPendingLikes();
  if (!pending.includes(videoId)) {
    pending.push(videoId);
    localStorage.setItem(PENDING_LIKES_KEY, JSON.stringify(pending));
  }
};

const removePendingLike = (videoId: string) => {
  if (typeof window === 'undefined') return;
  const pending = getPendingLikes();
  const filtered = pending.filter(id => id !== videoId);
  localStorage.setItem(PENDING_LIKES_KEY, JSON.stringify(filtered));
};

export const processPendingLikes = async () => {
  const pending = getPendingLikes();
  if (pending.length === 0) return;
  
  // Process all pending likes
  const results = await Promise.allSettled(
    pending.map(videoId => apiClient.post(`/api/videos/${videoId}/like`))
  );
  
  // Only remove successfully processed likes
  const successfulVideoIds: string[] = [];
  results.forEach((result, index) => {
    if (result.status === 'fulfilled') {
      successfulVideoIds.push(pending[index]);
    } else {
      console.error(`Failed to process pending like for video ${pending[index]}:`, result.reason);
    }
  });
  
  // Remove only successful likes from pending list
  if (successfulVideoIds.length > 0) {
    const remaining = pending.filter(id => !successfulVideoIds.includes(id));
    if (remaining.length === 0) {
      localStorage.removeItem(PENDING_LIKES_KEY);
    } else {
      localStorage.setItem(PENDING_LIKES_KEY, JSON.stringify(remaining));
    }
  }
};

export const useToggleLike = () => {
  const queryClient = useQueryClient();
  const { session } = useAuthSession();
  
  // Throttle state: track last like time per video
  const throttleRef = useRef<Map<string, number>>(new Map());
  const THROTTLE_MS = 2000; // 2 seconds throttle

  return useMutation({
    mutationFn: async (videoId: string): Promise<VideoDto> => {
      // Check throttle - silently ignore if throttled
      const lastLikeTime = throttleRef.current.get(videoId);
      const now = Date.now();
      if (lastLikeTime && (now - lastLikeTime) < THROTTLE_MS) {
        // Return current video data without making API call (throttled)
        const currentVideo = queryClient.getQueryData<VideoDto>(['video', videoId]);
        if (currentVideo) {
          return Promise.resolve(currentVideo);
        }
        // Fallback: return from videos list if available
        const videosData = queryClient.getQueryData<{ pages?: CursorPagedResult<VideoDto>[] }>({ queryKey: ['videos'] });
        if (videosData?.pages) {
          for (const page of videosData.pages) {
            const video = page.items.find(v => v.videoId === videoId);
            if (video) {
              return Promise.resolve(video);
            }
          }
        }
        // If no video data found, this is unexpected - but don't throw error, just return empty-like object
        return Promise.resolve({} as VideoDto);
      }

      // If not authenticated, store pending like and redirect to login
      if (!session?.userId) {
        addPendingLike(videoId);
        // Redirect to login with return URL
        const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
        window.location.href = `/login?returnUrl=${returnUrl}`;
        throw new Error("Please login to like videos");
      }

      // Update throttle
      throttleRef.current.set(videoId, now);

      // Optimistically update the video
      const currentVideo = queryClient.getQueryData<VideoDto>(['video', videoId]);
      if (currentVideo) {
        const optimisticVideo: VideoDto = {
          ...currentVideo,
          hasLiked: !currentVideo.hasLiked,
          likeCount: currentVideo.hasLiked 
            ? Math.max(0, currentVideo.likeCount - 1)
            : currentVideo.likeCount + 1
        };
        queryClient.setQueryData(['video', videoId], optimisticVideo);
      }

      // Also update in videos list (infinite query)
      queryClient.setQueriesData(
        { queryKey: ['videos'] },
        (old: any) => {
          if (!old) return old;
          // Handle infinite query structure
          if (old.pages) {
            return {
              ...old,
              pages: old.pages.map((page: CursorPagedResult<VideoDto>) => ({
                ...page,
                items: page.items.map(v => 
                  v.videoId === videoId
                    ? {
                        ...v,
                        hasLiked: !v.hasLiked,
                        likeCount: v.hasLiked 
                          ? Math.max(0, v.likeCount - 1)
                          : v.likeCount + 1
                      }
                    : v
                )
              }))
            };
          }
          // Handle regular query structure
          return {
            ...old,
            items: old.items.map((v: VideoDto) => 
              v.videoId === videoId
                ? {
                    ...v,
                    hasLiked: !v.hasLiked,
                    likeCount: v.hasLiked 
                      ? Math.max(0, v.likeCount - 1)
                      : v.likeCount + 1
                  }
                : v
            )
          };
        }
      );

      try {
        await apiClient.post(`/api/videos/${videoId}/like`);
        
        // Refetch to get accurate data
        await queryClient.invalidateQueries({ queryKey: ['video', videoId] });
        await queryClient.invalidateQueries({ queryKey: ['videos'] });
        
        // Return updated video
        const updatedVideo = queryClient.getQueryData<VideoDto>(['video', videoId]);
        if (updatedVideo) return updatedVideo;
        
        // Fallback: return optimistic update
        return currentVideo!;
      } catch (error) {
        // Revert optimistic update on error
        if (currentVideo) {
          queryClient.setQueryData(['video', videoId], currentVideo);
        }
        throw error;
      }
    }
  });
};