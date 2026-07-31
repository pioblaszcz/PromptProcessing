import { useEffect, useMemo } from "react";
import { getPromptJobStatuses } from "@/lib/prompt-jobs-api";
import type { PromptJob } from "@/lib/prompt-jobs.types";

const pollingIntervalInMilliseconds = 3_000;

type UsePromptJobStatusPollingOptions = {
  onStatusesReceived: (promptJobs: PromptJob[]) => void;
  promptJobs: PromptJob[];
};

export function usePromptJobStatusPolling({
  onStatusesReceived,
  promptJobs,
}: UsePromptJobStatusPollingOptions) {
  const activePromptJobIds = useMemo(
    () => promptJobs
      .filter((promptJob) => promptJob.status === "Pending" || promptJob.status === "Processing")
      .map((promptJob) => promptJob.id),
    [promptJobs],
  );

  useEffect(() => {
    if (activePromptJobIds.length === 0) {
      return;
    }

    let isActive = true;

    async function loadStatuses() {
      try {
        const updatedPromptJobs = await getPromptJobStatuses(activePromptJobIds);

        if (isActive) {
          onStatusesReceived(updatedPromptJobs);
        }
      } catch {
        // The next polling cycle retries the request.
      }
    }

    const intervalId = window.setInterval(() => {
      void loadStatuses();
    }, pollingIntervalInMilliseconds);

    return () => {
      isActive = false;
      window.clearInterval(intervalId);
    };
  }, [activePromptJobIds, onStatusesReceived]);
}
