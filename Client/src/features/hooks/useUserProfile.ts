import { useQuery } from "@tanstack/react-query";
import apiClient from "../../lib/api/axios";

export const useUserProfile = (userId: string | undefined) => {
  const { data: profile, isLoading } = useQuery({
    queryKey: ['userProfile', userId],
    queryFn: async () => {
      if (!userId) return null
      const response = await apiClient.get<UserProfileDto>(`/api/userprofiles/${userId}`)
      return response.data
    },
    enabled: !!userId
  })

  return { profile, isLoading }
}

