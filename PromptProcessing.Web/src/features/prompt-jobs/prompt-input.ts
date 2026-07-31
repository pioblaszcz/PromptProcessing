export function parsePrompts(value: string): string[] {
  return value
    .split(/\r?\n/)
    .map((prompt) => prompt.trim())
    .filter((prompt) => prompt.length > 0);
}
