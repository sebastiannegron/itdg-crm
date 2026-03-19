export type PageStatus = "idle" | "loading" | "success" | "failed";

export const urlRegex =
  /(https?:\/\/[^\s]+)|(www\.[^\s]+)|([a-zA-Z0-9-]+\.(com|net|org|edu|gov|io|biz|info|me))/i;

export const codeRegex =
  /<[^>]*>|{.*}|\[.*\]|function\s*\(|eval\(|=>/i;
