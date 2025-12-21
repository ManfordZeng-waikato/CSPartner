/**
 * Format a date string to a relative time string (e.g., "just now", "5m ago", "2h ago")
 * @param dateString - ISO date string (UTC)
 * @returns Formatted relative time string
 */
export const formatRelativeTime = (dateString: string): string => {
  try {
    // Parse the date string - ensure it's treated as UTC if it doesn't have timezone info
    let date: Date;
    
    // If the string doesn't end with Z or have timezone offset, assume it's UTC
    if (!dateString.includes('Z') && !dateString.match(/[+-]\d{2}:\d{2}$/)) {
      // Add 'Z' to indicate UTC if not present
      date = new Date(dateString.endsWith('Z') ? dateString : dateString + 'Z');
    } else {
      date = new Date(dateString);
    }
    
    const now = new Date();
    
    // Check if date is valid
    if (isNaN(date.getTime())) {
      console.warn('Invalid date string:', dateString);
      return "just now";
    }
    
    // Calculate time difference in seconds (both dates are in milliseconds since epoch)
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);
    
    // Handle negative time differences (future dates)
    if (diffInSeconds < 0) {
      // If the difference is small (less than 1 hour), it might be a timezone issue
      // In this case, treat it as "just now"
      if (Math.abs(diffInSeconds) < 3600) {
        return "just now";
      }
      console.warn('Future date detected:', dateString, 'diff:', diffInSeconds);
      return "just now";
    }

    // Format based on time difference
    if (diffInSeconds < 60) return "just now";
    if (diffInSeconds < 3600) return `${Math.floor(diffInSeconds / 60)}m ago`;
    if (diffInSeconds < 86400) return `${Math.floor(diffInSeconds / 3600)}h ago`;
    if (diffInSeconds < 604800) return `${Math.floor(diffInSeconds / 86400)}d ago`;
    
    // For dates older than a week, show the actual date
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  } catch (error) {
    console.error('Error formatting date:', error, dateString);
    return "just now";
  }
};

