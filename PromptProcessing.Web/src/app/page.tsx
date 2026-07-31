import { PromptJobsWorkspace } from "@/features/prompt-jobs/components/prompt-jobs-workspace";

export default function Home() {
  return (
    <main className="page-shell">
      <header className="page-header">
        <p className="product-name">Prompt Processing</p>
        <p className="product-description">Kolejka zadań dla modeli językowych</p>
      </header>

      <PromptJobsWorkspace />
    </main>
  );
}
