"use client";

import { useCallback, useEffect, useState } from "react";
import { createPromptJobs, getPromptJobs } from "@/lib/prompt-jobs-api";
import type { PromptJob } from "@/lib/prompt-jobs.types";
import { PromptJobComposer } from "@/features/prompt-jobs/components/prompt-job-composer";
import { PromptJobsQueue } from "@/features/prompt-jobs/components/prompt-jobs-queue";
import { usePromptJobStatusPolling } from "@/features/prompt-jobs/hooks/use-prompt-job-status-polling";

export function PromptJobsWorkspace() {
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [loadErrorMessage, setLoadErrorMessage] = useState<string | null>(null);
  const [promptJobs, setPromptJobs] = useState<PromptJob[]>([]);

  async function loadPromptJobs() {
    setLoadErrorMessage(null);
    setIsLoading(true);

    try {
      const page = await getPromptJobs();
      setPromptJobs(page.items);
    } catch (error) {
      setLoadErrorMessage(error instanceof Error ? error.message : "Nie udało się pobrać kolejki.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    let isActive = true;

    async function loadInitialPromptJobs() {
      try {
        const page = await getPromptJobs();

        if (isActive) {
          setPromptJobs(page.items);
        }
      } catch (error) {
        if (isActive) {
          setLoadErrorMessage(error instanceof Error ? error.message : "Nie udało się pobrać kolejki.");
        }
      } finally {
        if (isActive) {
          setIsLoading(false);
        }
      }
    }

    void loadInitialPromptJobs();

    return () => {
      isActive = false;
    };
  }, []);

  const handleStatusesReceived = useCallback((updatedPromptJobs: PromptJob[]) => {
    const updatedPromptJobsById = new Map(updatedPromptJobs.map((promptJob) => [promptJob.id, promptJob]));

    setPromptJobs((currentPromptJobs) => currentPromptJobs.map(
      (promptJob) => updatedPromptJobsById.get(promptJob.id) ?? promptJob,
    ));
  }, []);

  usePromptJobStatusPolling({
    onStatusesReceived: handleStatusesReceived,
    promptJobs,
  });

  async function handleCreatePromptJobs(prompts: string[]): Promise<boolean> {
    setErrorMessage(null);
    setIsSubmitting(true);

    try {
      await createPromptJobs(prompts);
      await loadPromptJobs();
      return true;
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : "Nie udało się dodać promptów.");
      return false;
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="workspace-grid">
      <PromptJobComposer
        errorMessage={errorMessage}
        isSubmitting={isSubmitting}
        onSubmit={handleCreatePromptJobs}
      />
      <PromptJobsQueue
        errorMessage={loadErrorMessage}
        isLoading={isLoading}
        promptJobs={promptJobs}
      />
    </div>
  );
}
