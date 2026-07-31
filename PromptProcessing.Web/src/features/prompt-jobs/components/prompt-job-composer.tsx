"use client";

import { FormEvent, useState } from "react";
import { parsePrompts } from "@/features/prompt-jobs/prompt-input";

type PromptJobComposerProps = {
  errorMessage: string | null;
  isSubmitting: boolean;
  onSubmit: (prompts: string[]) => Promise<boolean>;
};

export function PromptJobComposer({ errorMessage, isSubmitting, onSubmit }: PromptJobComposerProps) {
  const [value, setValue] = useState("");
  const [validationMessage, setValidationMessage] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const prompts = parsePrompts(value);

    if (prompts.length === 0) {
      setValidationMessage("Wprowadź co najmniej jeden prompt.");
      return;
    }

    setValidationMessage(null);

    if (await onSubmit(prompts)) {
      setValue("");
    }
  }

  const message = validationMessage ?? errorMessage;

  return (
    <section className="workspace-panel composer-panel" aria-labelledby="composer-heading">
      <div className="panel-index" aria-hidden="true">01</div>
      <div className="panel-content">
        <div className="panel-heading">
          <h1 id="composer-heading">Dodaj prompty</h1>
          <p>Jeden wiersz odpowiada jednemu zadaniu.</p>
        </div>

        <form className="prompt-form" onSubmit={handleSubmit}>
          <label className="prompt-input-label" htmlFor="prompts">
            Treść promptów
          </label>
          <textarea
            className="prompt-input"
            id="prompts"
            onChange={(event) => setValue(event.target.value)}
            placeholder="Wprowadź prompty do przetworzenia"
            value={value}
          />
          {message && <p className="form-message" role="alert">{message}</p>}
          <button className="primary-button" disabled={isSubmitting} type="submit">
            {isSubmitting ? "Dodawanie…" : "Dodaj do kolejki"}
          </button>
        </form>
      </div>
    </section>
  );
}
