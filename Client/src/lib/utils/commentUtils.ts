
/**
 * Recursively find and update a comment in the tree
 * Returns null if parent not found, otherwise returns updated comments array
 */
const findAndUpdateComment = (
  comments: CommentDto[],
  parentCommentId: string,
  newComment: CommentDto
): CommentDto[] | null => {
  for (let i = 0; i < comments.length; i++) {
    const comment = comments[i];
    if (comment.commentId === parentCommentId) {
      // Found the parent, add the new comment to its replies
      const updatedComment = {
        ...comment,
        replies: [newComment, ...(comment.replies || [])]
      };
      // Return new array with updated comment
      return [
        ...comments.slice(0, i),
        updatedComment,
        ...comments.slice(i + 1)
      ];
    } else if (comment.replies && comment.replies.length > 0) {
      // Recursively search in replies
      const updatedReplies = findAndUpdateComment(comment.replies, parentCommentId, newComment);
      if (updatedReplies !== null) {
        // Found and updated in replies
        return [
          ...comments.slice(0, i),
          {
            ...comment,
            replies: updatedReplies
          },
          ...comments.slice(i + 1)
        ];
      }
    }
  }
  return null; // Parent not found
};

/**
 * Find the root comment ID for a given comment ID
 * Returns the root comment ID (the one with parentCommentId === null)
 */
const findRootCommentId = (
  comments: CommentDto[],
  targetCommentId: string
): string | null => {
  // Build a map of all comments for quick lookup
  const commentMap = new Map<string, CommentDto>();
  const buildMap = (comments: CommentDto[]) => {
    for (const comment of comments) {
      commentMap.set(comment.commentId, comment);
      if (comment.replies && comment.replies.length > 0) {
        buildMap(comment.replies);
      }
    }
  };
  buildMap(comments);

  // Find the target comment
  const targetComment = commentMap.get(targetCommentId);
  if (!targetComment) {
    console.warn('Target comment not found:', targetCommentId);
    return null;
  }

  // If the target comment is a root comment, return its ID
  if (!targetComment.parentCommentId) {
    return targetComment.commentId;
  }

  // Traverse up the tree to find the root comment
  let currentComment: CommentDto | undefined = targetComment;
  const visited = new Set<string>(); // Prevent infinite loops

  while (currentComment && currentComment.parentCommentId) {
    if (visited.has(currentComment.commentId)) {
      console.warn('Circular reference detected in comment tree');
      return null;
    }
    visited.add(currentComment.commentId);

    const parentId = currentComment.parentCommentId;
    currentComment = commentMap.get(parentId);

    if (!currentComment) {
      console.warn('Parent comment not found:', parentId);
      return null;
    }

    // If we found a root comment, return its ID
    if (!currentComment.parentCommentId) {
      return currentComment.commentId;
    }
  }

  return null;
};

/**
 * Add a new comment to the existing comments list
 * Handles both top-level comments and nested replies
 * For replies, all replies are added to the root comment's replies array (flat structure)
 * @param prevComments - Current comments list
 * @param newComment - New comment to add
 * @returns Updated comments list
 */
export const addCommentToList = (
  prevComments: CommentDto[],
  newComment: CommentDto
): CommentDto[] => {
  // Check if comment already exists (avoid duplicates)
  const checkExists = (comments: CommentDto[]): boolean => {
    for (const comment of comments) {
      if (comment.commentId === newComment.commentId) {
        return true;
      }
      if (comment.replies && comment.replies.length > 0) {
        if (checkExists(comment.replies)) {
          return true;
        }
      }
    }
    return false;
  };

  if (checkExists(prevComments)) {
    return prevComments;
  }

  // If it's a reply, find the root comment and add it to the root's replies
  if (newComment.parentCommentId) {
    // Find the root comment ID
    const rootCommentId = findRootCommentId(prevComments, newComment.parentCommentId);
    
    if (rootCommentId) {
      // Add the reply to the root comment's replies array
      const updated = findAndUpdateComment(prevComments, rootCommentId, newComment);
      if (updated !== null) {
        return updated;
      }
    }
    
    // If root not found, try to add to the direct parent (fallback)
    // This handles the case where the parent might be a reply itself
    const updated = findAndUpdateComment(prevComments, newComment.parentCommentId, newComment);
    if (updated !== null) {
      return updated;
    }
    
    // If parent not found, it might be a new reply to a comment that hasn't loaded yet
    // The full comment list refresh from SignalR will handle it
    // Don't add it as a top-level comment, just return the original list
    // The full comment list refresh from SignalR will handle it
    return prevComments;
  }

  // It's a top-level comment, add it to the beginning of the list (newest first)
  return [newComment, ...prevComments];
};

