import React from "react";
import { TextField, type TextFieldProps } from "@mui/material";
import { type UseFormRegisterReturn, type FieldError } from "react-hook-form";

export interface FormTextFieldProps extends Omit<TextFieldProps, "error" | "helperText"> {
  register: UseFormRegisterReturn;
  error?: FieldError;
  helperText?: string;
}

/**
 * 通用的表单 TextField 组件，自动处理 react-hook-form 的注册和错误显示
 */
const FormTextField: React.FC<FormTextFieldProps> = ({
  register,
  error,
  helperText,
  ...textFieldProps
}) => {
  return (
    <TextField
      {...register}
      {...textFieldProps}
      error={!!error}
      helperText={error?.message || helperText}
    />
  );
};

export default FormTextField;

