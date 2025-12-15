import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import axios from "axios";

export const useVideos=()=> {
    const queryClient = useQueryClient()
    const { data: videos, isLoading } = useQuery({
        queryKey: ['videos'],
        queryFn: async () => {
          const response = await axios.get<VideoDto[]>('/api/videos')
          return response.data
        }
      });

      const createVideo = useMutation({
        mutationFn: async (video: CreateVideoDto) => {
          const formData = new FormData()
          formData.append('VideoStream', video.videoStream)
          formData.append('VideoFileName', video.videoFileName)
          if (video.thumbnailStream) {
            formData.append('ThumbnailStream', video.thumbnailStream)
          }
          if (video.thumbnailFileName) {
            formData.append('ThumbnailFileName', video.thumbnailFileName)
          }
          formData.append('Title', video.title)
          if (video.description !== undefined && video.description !== null) {
            formData.append('Description', video.description)
          }
          formData.append('Visibility', video.visibility.toString())

          await axios.post('/api/videos/upload', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
          })
        },
        onSuccess:async () => {
          await queryClient.invalidateQueries({ queryKey: ['videos'] })
        }
      })

      return { videos, isLoading, createVideo }
}