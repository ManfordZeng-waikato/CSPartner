import { useEffect, useRef, useState } from 'react';

interface UseVideoLazyLoadOptions {
  rootMargin?: string;
  threshold?: number;
}

/**
 * Custom hook for lazy loading videos when they enter the viewport
 * @param rootMargin - Margin around the viewport (default: '200px' to preload slightly before visible)
 * @param threshold - Percentage of visibility to trigger (default: 0.1)
 */
export function useVideoLazyLoad(options: UseVideoLazyLoadOptions = {}) {
  const { rootMargin = '200px', threshold = 0.1 } = options;
  const videoRef = useRef<HTMLVideoElement>(null);
  const [shouldLoad, setShouldLoad] = useState(false);

  useEffect(() => {
    const videoElement = videoRef.current;
    if (!videoElement) return;

    // Create intersection observer
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            // Video is visible or near visible, load it
            setShouldLoad(true);
            // Once loaded, we can stop observing
            observer.unobserve(videoElement);
          }
        });
      },
      {
        rootMargin,
        threshold,
      }
    );

    observer.observe(videoElement);

    return () => {
      observer.disconnect();
    };
  }, [rootMargin, threshold]);

  return { videoRef, shouldLoad };
}

