// Enums
enum VideoVisibility {
    Public = 1,
    Unlisted = 2,
    Private = 3
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
    videoStream: File
    videoFileName: string
    thumbnailStream?: File | null
    thumbnailFileName?: string | null
    title: string
    description?: string | null
    visibility: VideoVisibility
    uploaderUserId: string
}

type UpdateVideoDto = {
    title: string | null
    description: string | null
    thumbnailUrl: string | null
    visibility: VideoVisibility | null
}

type UploadVideoResponseDto = {
    videoUrl: string
    thumbnailUrl: string | null
    message: string
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
    content: string
    createdAtUtc: string
    updatedAtUtc: string | null
    replies: CommentDto[]
}

type CreateCommentDto = {
    content: string
    parentCommentId: string | null
}
