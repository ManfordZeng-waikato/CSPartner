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
          await axios.post('/api/videos/upload', video)
        },
        onSuccess:async () => {
          await queryClient.invalidateQueries({ queryKey: ['videos'] })
        }
      })

      return { videos, isLoading, createVideo }
}
