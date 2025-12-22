// CommentDto type definition
interface CommentDto {
  commentId: string;
  videoId: string;
  userId: string;
  parentCommentId: string | null;
  content: string;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  replies: CommentDto[];
}

/**
 * Add a new comment to the existing comments list
 * Handles both top-level comments and replies
 * @param prevComments - Current comments list
 * @param newComment - New comment to add
 * @returns Updated comments list
 */
export const addCommentToList = (
  prevComments: CommentDto[],
  newComment: CommentDto
): CommentDto[] => {
  // Check if comment already exists (avoid duplicates)
  const exists = prevComments.some(c => c.commentId === newComment.commentId);
  if (exists) {
    return prevComments;
  }

  // If it's a reply, add it to the parent comment's replies (newest first)
  if (newComment.parentCommentId) {
    return prevComments.map(comment => {
      if (comment.commentId === newComment.parentCommentId) {
        return {
          ...comment,
          replies: [newComment, ...(comment.replies || [])]
        };
      }
      return comment;
    });
  }

  // It's a top-level comment, add it to the beginning of the list (newest first)
  return [newComment, ...prevComments];
};

