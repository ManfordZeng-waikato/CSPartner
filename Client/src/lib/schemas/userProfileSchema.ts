import { z } from "zod";

// Helper function to validate URL
const validateUrl = (val: string | null | undefined): boolean => {
  if (!val || val === "") return true;
  try {
    new URL(val);
    return true;
  } catch {
    return false;
  }
};

export const updateProfileSchema = z.object({
  displayName: z
    .string()
    .trim()
    .min(2, "Display name must be at least 2 characters")
    .max(50, "Display name must be at most 50 characters")
    .optional()
    .nullable(),
  bio: z
    .string()
    .trim()
    .max(500, "Bio must be at most 500 characters")
    .optional()
    .nullable(),
  avatarUrl: z.string().optional().nullable(),
  steamProfileUrl: z
    .string()
    .refine(validateUrl, { message: "Please enter a valid URL" })
    .optional()
    .nullable()
    .or(z.literal("")),
  faceitProfileUrl: z
    .string()
    .refine(validateUrl, { message: "Please enter a valid URL" })
    .optional()
    .nullable()
    .or(z.literal(""))
});

export type UpdateProfileFormValues = z.infer<typeof updateProfileSchema>;

