/**
 * 获取用户头像URL
 * 如果用户没有设置头像，则返回默认头像 icon1.png
 */
export const getAvatarUrl = (avatarUrl: string | null | undefined): string => {
  // 检查 avatarUrl 是否为有效字符串（非空、非null、非undefined）
  if (avatarUrl && avatarUrl.trim() !== "") {
    return avatarUrl;
  }
  return "/Icon/icon1.png";
};

