import { DEFAULT_AVATAR_URL } from "../constants/avatars";

/**
 * Get user avatar URL
 * If user has not set an avatar, return the default avatar icon1.png
 */
export const getAvatarUrl = (avatarUrl: string | null | undefined): string => {
  // Check if avatarUrl is a valid string (non-empty, non-null, non-undefined)
  if (avatarUrl && avatarUrl.trim() !== "") {
    return avatarUrl;
  }
  return DEFAULT_AVATAR_URL;
};

