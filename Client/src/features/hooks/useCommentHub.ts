import { useEffect, useRef, useState } from "react";
import * as signalR from "@microsoft/signalr";
import { getAuthToken } from "../../lib/api/axios";

interface UseCommentHubOptions {
  videoId: string | undefined;
  onCommentsReceived: (comments: CommentDto[]) => void;
  onNewCommentReceived?: (comment: CommentDto) => void;
  enabled?: boolean;
}

export const useCommentHub = ({ videoId, onCommentsReceived, onNewCommentReceived, enabled = true }: UseCommentHubOptions) => {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const onCommentsReceivedRef = useRef(onCommentsReceived);
  const onNewCommentReceivedRef = useRef(onNewCommentReceived);
  const isCleaningUpRef = useRef(false);
  const isMountedRef = useRef(true);
  const [isConnected, setIsConnected] = useState(false);

  // Keep the callback refs up to date
  useEffect(() => {
    onCommentsReceivedRef.current = onCommentsReceived;
  }, [onCommentsReceived]);

  useEffect(() => {
    onNewCommentReceivedRef.current = onNewCommentReceived;
  }, [onNewCommentReceived]);

  useEffect(() => {
    if (!enabled || !videoId) {
      return;
    }

    // Reset flags
    isCleaningUpRef.current = false;
    isMountedRef.current = true;

    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || "";
    const hubPath = "/api/hubs/comments";
    const token = getAuthToken();

    // Build connection URL with videoId and optional token
    let hubUrl: string;
    if (apiBaseUrl) {
      // If base URL is provided, use it
      const baseUrl = apiBaseUrl.endsWith('/') ? apiBaseUrl.slice(0, -1) : apiBaseUrl;
      hubUrl = `${baseUrl}${hubPath}`;
    } else {
      // Relative URL
      hubUrl = hubPath;
    }

    // Add query parameters
    const separator = hubUrl.includes('?') ? '&' : '?';
    hubUrl += `${separator}videoId=${encodeURIComponent(videoId)}`;
    if (token) {
      hubUrl += `&access_token=${encodeURIComponent(token)}`;
    }

    // Create SignalR connection
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .configureLogging(signalR.LogLevel.Warning)
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0s, 2s, 10s, 30s, then 30s max
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        }
      })
      .build();

    // Set up event handlers using ref to avoid stale closures
    connection.on("LoadComments", (comments: CommentDto[]) => {
      if (isMountedRef.current && !isCleaningUpRef.current) {
        onCommentsReceivedRef.current(comments);
      }
    });

    connection.on("ReceiveComments", (comments: CommentDto[]) => {
      if (isMountedRef.current && !isCleaningUpRef.current) {
        onCommentsReceivedRef.current(comments);
      }
    });

    connection.on("ReceiveNewComment", (comment: CommentDto) => {
      if (isMountedRef.current && !isCleaningUpRef.current && onNewCommentReceivedRef.current) {
        onNewCommentReceivedRef.current(comment);
      }
    });

    connection.onreconnecting(() => {
      if (isMountedRef.current && !isCleaningUpRef.current) {
        setIsConnected(false);
      }
    });

    connection.onreconnected(() => {
      if (isMountedRef.current && !isCleaningUpRef.current) {
        setIsConnected(true);
      }
    });

    connection.onclose(() => {
      if (isMountedRef.current && !isCleaningUpRef.current) {
        setIsConnected(false);
      }
    });

    // Store connection reference before starting
    connectionRef.current = connection;

    // Start connection asynchronously
    const startPromise = connection.start()
      .then(() => {
        if (isMountedRef.current && connectionRef.current === connection && !isCleaningUpRef.current) {
          setIsConnected(true);
        }
      })
      .catch((error) => {
        // Only log unexpected errors
        if (isMountedRef.current && connectionRef.current === connection && !isCleaningUpRef.current) {
          if (error.name !== 'AbortError' && error.message !== 'The connection was stopped during negotiation.') {
            console.error("SignalR connection error:", error);
          }
        }
      });

    // Cleanup on unmount or videoId change
    return () => {
      isCleaningUpRef.current = true;
      isMountedRef.current = false;
      
      // Only cleanup if this is still the active connection
      if (connectionRef.current === connection) {
        connectionRef.current = null;
        setIsConnected(false);

        // Wait for start to complete (or fail) before stopping
        startPromise
          .finally(() => {
            // Only stop if connection is in a state that allows stopping
            if (connection && connection.state !== signalR.HubConnectionState.Disconnected) {
              connection.stop()
                .then(() => {
                  // Silently complete - component is unmounting
                })
                .catch(() => {
                  // Silently ignore errors during cleanup
                });
            }
          });
      }
    };
  }, [videoId, enabled]);

  return {
    isConnected
  };
};

