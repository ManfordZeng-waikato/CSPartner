import React, { type ReactNode } from "react";
import { Box, Paper, Typography, Alert } from "@mui/material";

export interface FormContainerProps {
  title: string;
  subtitle?: string;
  children: ReactNode;
  maxWidth?: number | string;
  elevation?: number;
  errorMessage?: ReactNode;
  successMessage?: ReactNode;
  infoMessage?: ReactNode;
  onCloseError?: () => void;
  onCloseSuccess?: () => void;
  onCloseInfo?: () => void;
  sx?: Record<string, any>;
  paperSx?: Record<string, any>;
}

/**
 * 通用的表单容器组件，提供统一的布局和样式
 * 包含标题、副标题、Alert 消息和表单内容区域
 */
const FormContainer: React.FC<FormContainerProps> = ({
  title,
  subtitle,
  children,
  maxWidth = 420,
  elevation = 3,
  errorMessage,
  successMessage,
  infoMessage,
  onCloseError,
  onCloseSuccess,
  onCloseInfo,
  sx,
  paperSx
}) => {
  return (
    <Box sx={{ display: "flex", justifyContent: "center", mt: 8, px: 2, ...sx }}>
      <Paper sx={{ p: 4, maxWidth, width: "100%", elevation, ...paperSx }}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          {title}
        </Typography>
        {subtitle && (
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            {subtitle}
          </Typography>
        )}

        {errorMessage && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={onCloseError}>
            {errorMessage}
          </Alert>
        )}
        {successMessage && (
          <Alert severity="success" sx={{ mb: 2 }} onClose={onCloseSuccess}>
            {successMessage}
          </Alert>
        )}
        {infoMessage && (
          <Alert severity="info" sx={{ mb: 2 }} onClose={onCloseInfo}>
            {infoMessage}
          </Alert>
        )}

        {children}
      </Paper>
    </Box>
  );
};

export default FormContainer;

