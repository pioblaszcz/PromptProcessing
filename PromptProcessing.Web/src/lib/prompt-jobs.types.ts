export type PromptJobStatus =
  | "Pending"
  | "Processing"
  | "Completed"
  | "Failed";

export type CreatedPromptJob = {
  id: string;
  content: string;
};

export type PromptJob = {
  id: string;
  content: string;
  status: PromptJobStatus;
  result: string | null;
  errorMessage: string | null;
  createdAtUtc: string;
  processingStartedAtUtc: string | null;
  completedAtUtc: string | null;
  attemptCount: number;
};

export type PromptJobsPage = {
  items: PromptJob[];
  nextCursor: string | null;
};
