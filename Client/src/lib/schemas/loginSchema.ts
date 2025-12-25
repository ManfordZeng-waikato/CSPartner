import { z } from "zod";

export const loginSchema = z.object({
  email: z.string().trim().email({ message: "Please enter a valid email" }),
  password: z.string().min(6, "Password must be at least 6 characters"),
  rememberMe: z.boolean().optional()
});

export type LoginFormValues = z.infer<typeof loginSchema>;

export const registerSchema = z
  .object({
    email: z.string().trim().email({ message: "Please enter a valid email" }),
    password: z
      .string()
      .min(8, "Password must be at least 8 characters")
      .regex(/[0-9]/, "Password must contain at least one number")
      .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
      .regex(/[^a-zA-Z0-9]/, "Password must contain at least one special character"),
    confirmPassword: z
      .string()
      .min(8, "Confirm password must be at least 8 characters"),
    displayName: z
      .string()
      .trim()
      .min(2, "Display name must be at least 2 characters")
      .max(50, "Display name must be at most 50 characters")
      .optional(),
    avatarUrl: z.string().optional()
  })
  .refine((data) => data.password === data.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords must match"
  });

export type RegisterFormValues = z.infer<typeof registerSchema>;

export const requestPasswordResetSchema = z.object({
  email: z.string().trim().email({ message: "Please enter a valid email" })
});

export type RequestPasswordResetFormValues = z.infer<typeof requestPasswordResetSchema>;

export const resetPasswordSchema = z
  .object({
    email: z.string().trim().email({ message: "Please enter a valid email" }),
    newPassword: z
      .string()
      .min(8, "Password must be at least 8 characters")
      .regex(/[0-9]/, "Password must contain at least one number")
      .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
      .regex(/[^a-zA-Z0-9]/, "Password must contain at least one special character"),
    confirmPassword: z
      .string()
      .min(8, "Confirm password must be at least 8 characters"),
    code: z.string().min(1, "Reset code is required")
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords must match"
  });

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>;