import { useQuery } from "@tanstack/react-query";
import axios from "axios";

export const useVideos=()=> {
    const { data: videos, isLoading } = useQuery({
        queryKey: ['videos'],
        queryFn: async () => {
          const response = await axios.get<VideoDto[]>('/api/videos')
          return response.data
        }
      });

      return { videos, isLoading }
}
