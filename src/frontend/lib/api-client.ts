export async function parseResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { message?: string } | null;
    throw new Error(body?.message ?? "Erro HTTP " + response.status);
  }

  return response.json() as Promise<T>;
}
