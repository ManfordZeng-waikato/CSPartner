import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    primary: {
      main: '#FF6B35', // 橙色 - 来自 CS2 背景
      light: '#FF8C65',
      dark: '#E55100',
      contrastText: '#FFFFFF', // 白色文字在橙色背景上
    },
    secondary: {
      main: '#424242', // 深灰色 - 来自人物剪影
      light: '#616161',
      dark: '#212121',
      contrastText: '#FFFFFF', // 白色文字
    },
    background: {
      default: '#F5F5F5', // 浅灰色背景 - 来自图片左半部分
      paper: '#FFFFFF',
    },
    text: {
      primary: '#212121', // 深灰色文字
      secondary: '#757575', // 中等灰色文字
    },
    divider: '#BDBDBD', // 灰色分隔线
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

