import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import '@fontsource/roboto/300.css';
import '@fontsource/roboto/400.css';
import '@fontsource/roboto/500.css';
import '@fontsource/roboto/700.css';
import './app/layout/styles.css'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { RouterProvider } from 'react-router';
import { router } from './app/router/Routes.tsx';
import { ThemeProvider } from '@mui/material/styles';
import { theme } from './app/theme/theme';
import { getAuthToken } from './lib/api/axios';
import { loadSession, clearSession } from './features/hooks/useAccount';

// Initialize: Ensure token and session are in sync
// If token doesn't exist but session does, clear the session
const initializeAuth = () => {
  const token = getAuthToken();
  const session = loadSession();
  
  // If token is missing but session exists, clear the session
  // This handles cases where token expired or was manually removed
  if (!token && session) {
    clearSession();
  }
};

// Run initialization before rendering
initializeAuth();

const queryClient = new QueryClient();
createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={queryClient}>
        <ReactQueryDevtools />
        <RouterProvider router={router} />
      </QueryClientProvider>
    </ThemeProvider>
  </StrictMode>,
)
