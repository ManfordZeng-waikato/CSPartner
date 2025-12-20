import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
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

export const useUpdateUserProfile = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ userId, data }: { userId: string; data: UpdateUserProfileDto }): Promise<UserProfileDto> => {
      const response = await apiClient.put<UserProfileDto>(`/api/userprofiles/${userId}`, data);
      return response.data;
    },
    onSuccess: (data) => {
      // Invalidate and refetch user profile
      queryClient.invalidateQueries({ queryKey: ['userProfile', data.userId] });
    }
  });
}

