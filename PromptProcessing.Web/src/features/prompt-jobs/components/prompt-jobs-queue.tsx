import type { PromptJob, PromptJobStatus } from "@/lib/prompt-jobs.types";

type PromptJobsQueueProps = {
  errorMessage: string | null;
  isLoading: boolean;
  promptJobs: PromptJob[];
};

const statusLabels: Record<PromptJobStatus, string> = {
  Completed: "Zakończone",
  Failed: "Nieudane",
  Pending: "Oczekujące",
  Processing: "Przetwarzane",
};

const dateTimeFormatter = new Intl.DateTimeFormat("pl-PL", {
  dateStyle: "medium",
  timeStyle: "short",
});

export function PromptJobsQueue({ errorMessage, isLoading, promptJobs }: PromptJobsQueueProps) {
  return (
    <section className="workspace-panel queue-panel" aria-labelledby="queue-heading">
      <div className="panel-index" aria-hidden="true">02</div>
      <div className="panel-content">
        <div className="panel-heading">
          <h2 id="queue-heading">Kolejka zadań</h2>
          <p>Najnowsze prompty i ich aktualny status.</p>
        </div>

        {isLoading && <div className="empty-queue" role="status">Ładowanie kolejki…</div>}
        {errorMessage && <p className="queue-message" role="alert">{errorMessage}</p>}
        {!isLoading && !errorMessage && promptJobs.length === 0 && (
          <div className="empty-queue" role="status">
            Zadania pojawią się tutaj po dodaniu pierwszych promptów.
          </div>
        )}
        {!isLoading && !errorMessage && promptJobs.length > 0 && (
          <ul className="prompt-jobs-list">
            {promptJobs.map((promptJob) => (
              <li className="prompt-job" key={promptJob.id}>
                <div className="prompt-job-header">
                  <span className={`status-badge status-${promptJob.status.toLowerCase()}`}>
                    {statusLabels[promptJob.status]}
                  </span>
                  <time dateTime={promptJob.createdAtUtc}>
                    {dateTimeFormatter.format(new Date(promptJob.createdAtUtc))}
                  </time>
                </div>
                <p className="prompt-job-content">{promptJob.content}</p>
                {promptJob.result && <p className="prompt-job-result">{promptJob.result}</p>}
                {promptJob.errorMessage && <p className="prompt-job-error">{promptJob.errorMessage}</p>}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  );
}
