import type {
  CreatedPromptJob,
  PromptJob,
  PromptJobsPage,
} from "@/lib/prompt-jobs.types";

type ProblemDetails = {
  detail?: string;
  errors?: Record<string, string[]>;
  title?: string;
};

type GetPromptJobsOptions = {
  cursor?: string;
  take?: number;
};

function getApiUrl(): string {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL;

  if (!apiUrl) {
    throw new Error("NEXT_PUBLIC_API_URL is not configured.");
  }

  return apiUrl.replace(/\/$/, "");
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${getApiUrl()}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers,
    },
  });

  if (!response.ok) {
    throw new Error(await getErrorMessage(response));
  }

  return response.json() as Promise<T>;
}

async function getErrorMessage(response: Response): Promise<string> {
  const problemDetails = (await response.json().catch(() => null)) as ProblemDetails | null;

  if (problemDetails?.errors) {
    return Object.values(problemDetails.errors).flat().join(" ");
  }

  return problemDetails?.detail ?? problemDetails?.title ?? "The request could not be completed.";
}

export function createPromptJobs(prompts: string[]): Promise<CreatedPromptJob[]> {
  return request<CreatedPromptJob[]>("/api/prompt-jobs", {
    body: JSON.stringify({ prompts }),
    method: "POST",
  });
}

export function getPromptJobs({ cursor, take = 20 }: GetPromptJobsOptions = {}): Promise<PromptJobsPage> {
  const searchParams = new URLSearchParams({ take: take.toString() });

  if (cursor) {
    searchParams.set("cursor", cursor);
  }

  return request<PromptJobsPage>(`/api/prompt-jobs?${searchParams}`, {
    cache: "no-store",
  });
}

export function getPromptJobStatuses(ids: string[]): Promise<PromptJob[]> {
  if (ids.length === 0) {
    return Promise.resolve([]);
  }

  const searchParams = new URLSearchParams();

  ids.forEach((id) => searchParams.append("ids", id));

  return request<PromptJob[]>(`/api/prompt-jobs/status?${searchParams}`, {
    cache: "no-store",
  });
}
