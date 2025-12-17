import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    primary: {
      main: '#FF6B35', // Orange accent inspired by CS2 background
      light: '#FF8C65',
      dark: '#E55100',
      contrastText: '#FFFFFF', // White text on orange
    },
    secondary: {
      main: '#424242', // Dark gray inspired by silhouette
      light: '#616161',
      dark: '#212121',
      contrastText: '#FFFFFF', // White text
    },
    background: {
      default: '#F5F5F5', // Light gray background
      paper: '#FFFFFF',
    },
    text: {
      primary: '#212121', // Dark gray text
      secondary: '#757575', // Medium gray text
    },
    divider: '#BDBDBD', // Gray divider
  },
  typography: {
    fontFamily: 'Roboto, Arial, sans-serif',
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          borderRadius: 4,
        },
        containedPrimary: {
          backgroundColor: '#FF6B35',
          color: '#FFFFFF',
          '&:hover': {
            backgroundColor: '#E55100',
          },
        },
        containedSecondary: {
          backgroundColor: '#424242',
          color: '#FFFFFF',
          '&:hover': {
            backgroundColor: '#212121',
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: '#FF6B35 !important',
          color: '#FFFFFF !important',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundColor: '#FFFFFF',
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          '&.MuiChip-outlinedPrimary': {
            borderColor: '#FF6B35',
            color: '#FF6B35',
            '&:hover': {
              backgroundColor: '#FFF3E0',
            },
          },
        },
      },
    },
    MuiDivider: {
      styleOverrides: {
        root: {
          borderColor: '#BDBDBD',
        },
      },
    },
  },
});

