// Enums
enum VideoVisibility {
    Public = 1,
    Private = 2
}

// Pagination Types
type CursorPagedResult<T> = {
    items: T[]
    nextCursor: string | null
    hasMore: boolean
    count: number
}

// Video Types
type VideoDto = {
    videoId: string
    uploaderUserId: string
    title: string
    description: string | null
    videoUrl: string
    thumbnailUrl: string | null
    likeCount: number
    commentCount: number
    viewCount: number
    visibility: VideoVisibility
    createdAtUtc: string
    updatedAtUtc: string | null
    hasLiked: boolean
}

type CreateVideoDto = {
    videoFile: File
    title: string
    description?: string | null
    visibility: VideoVisibility
}

type CreateVideoRequestDto = {
    title: string
    description?: string | null
    videoObjectKey: string
    thumbnailObjectKey?: string | null
    visibility: VideoVisibility
}

type VideoUploadUrlResponseDto = {
    uploadUrl: string
    objectKey: string
    publicUrl: string
    expiresAtUtc: string
    contentType: string
    method: string
}

type UpdateVideoDto = {
    title: string | null
    description: string | null
    thumbnailUrl: string | null
    visibility: VideoVisibility | null
}

// User Profile Types
type UserProfileDto = {
    id: string
    userId: string
    displayName: string | null
    bio: string | null
    avatarUrl: string | null
    steamProfileUrl: string | null
    faceitProfileUrl: string | null
    createdAtUtc: string
    updatedAtUtc: string | null
    videos: readonly VideoDto[]
}

type UpdateUserProfileDto = {
    displayName: string | null
    bio: string | null
    avatarUrl: string | null
    steamProfileUrl: string | null
    faceitProfileUrl: string | null
}

// Comment Types
type CommentDto = {
    commentId: string
    videoId: string
    userId: string
    parentCommentId: string | null
    parentUserId: string | null
    content: string
    createdAtUtc: string
    updatedAtUtc: string | null
    replies: CommentDto[]
}

type CreateCommentDto = {
    content: string
    parentCommentId: string | null
}
