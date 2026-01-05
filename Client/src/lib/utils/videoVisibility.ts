/**
 * Helper function to check if a video visibility is Public
 * Supports both string ("public"/"private") and number (1/2) formats
 */
export function isVideoPublic(visibility: string | number): boolean {
  if (typeof visibility === 'number') {
    return visibility === 1;
  }
  if (typeof visibility === 'string') {
    return visibility.toLowerCase() === 'public' || visibility === '1';
  }
  return false;
}

