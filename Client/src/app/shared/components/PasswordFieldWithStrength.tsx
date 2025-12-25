import React, { useMemo } from "react";
import { Box, TextField, Typography, LinearProgress } from "@mui/material";
import { type UseFormRegisterReturn, type FieldError, type Control, useWatch } from "react-hook-form";

export interface PasswordFieldWithStrengthProps {
  register: UseFormRegisterReturn;
  control: Control<any>;
  passwordFieldName: string;
  error?: FieldError;
  helperText?: string;
  label?: string;
  disabled?: boolean;
  fullWidth?: boolean;
  showStrengthIndicator?: boolean;
  showRequirements?: boolean;
}

/**
 * 带密码强度验证的密码输入字段组件
 */
const PasswordFieldWithStrength: React.FC<PasswordFieldWithStrengthProps> = ({
  register,
  control,
  passwordFieldName,
  error,
  helperText,
  label = "Password",
  disabled = false,
  fullWidth = true,
  showStrengthIndicator = true,
  showRequirements = true
}) => {
  const password = useWatch({ control, name: passwordFieldName });

  // Password strength requirements
  const passwordRequirements = useMemo(() => {
    return {
      minLength: password ? password.length >= 8 : false,
      hasNumber: password ? /[0-9]/.test(password) : false,
      hasUppercase: password ? /[A-Z]/.test(password) : false,
      hasSpecialChar: password ? /[^a-zA-Z0-9]/.test(password) : false
    };
  }, [password]);

  const passwordStrength = useMemo(() => {
    if (!password) return 0;
    let strength = 0;
    if (password.length >= 8) strength += 25;
    if (/[0-9]/.test(password)) strength += 25;
    if (/[A-Z]/.test(password)) strength += 25;
    if (/[^a-zA-Z0-9]/.test(password)) strength += 25;
    return strength;
  }, [password]);

  const getPasswordStrengthColor = (): "error" | "warning" | "success" => {
    if (passwordStrength < 25) return "error";
    if (passwordStrength < 50) return "warning";
    if (passwordStrength < 75) return "warning";
    return "success";
  };

  const hasRequirementsError = password && !Object.values(passwordRequirements).every(v => v);

  return (
    <Box>
      <TextField
        {...register}
        label={label}
        type="password"
        fullWidth={fullWidth}
        error={!!error || hasRequirementsError}
        helperText={error?.message || helperText}
        disabled={disabled}
      />
      {(showStrengthIndicator || showRequirements) && (
        <Box sx={{ mt: 1 }}>
          {showStrengthIndicator && password && (
            <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
              <LinearProgress
                variant="determinate"
                value={passwordStrength}
                color={getPasswordStrengthColor()}
                sx={{ flexGrow: 1, height: 6, borderRadius: 3 }}
              />
              <Typography variant="caption" color="text.secondary" sx={{ minWidth: 40 }}>
                {passwordStrength}%
              </Typography>
            </Box>
          )}
          {showRequirements && (
            <Box sx={{ mt: password ? 1 : 0 }}>
              <Typography variant="caption" color="text.secondary" sx={{ display: "block", mb: 0.5 }}>
                Password requirements:
              </Typography>
              <Box component="ul" sx={{ m: 0, pl: 2, fontSize: "0.75rem" }}>
                <Box
                  component="li"
                  sx={{
                    color: passwordRequirements.minLength ? "success.main" : "error.main"
                  }}
                >
                  At least 8 characters
                </Box>
                <Box
                  component="li"
                  sx={{
                    color: passwordRequirements.hasNumber ? "success.main" : "error.main"
                  }}
                >
                  At least one number
                </Box>
                <Box
                  component="li"
                  sx={{
                    color: passwordRequirements.hasUppercase ? "success.main" : "error.main"
                  }}
                >
                  At least one uppercase letter
                </Box>
                <Box
                  component="li"
                  sx={{
                    color: passwordRequirements.hasSpecialChar ? "success.main" : "error.main"
                  }}
                >
                  At least one special character
                </Box>
              </Box>
            </Box>
          )}
        </Box>
      )}
    </Box>
  );
};

export default PasswordFieldWithStrength;

