export async function listarEnfermeiras(): Promise<string[]> {
    const resposta = await fetch('/api/enfermeiras')
  if (!resposta.ok) {
    throw new Error(`Erro ${resposta.status} ao buscar as enfermeiras`)
  }
  return await resposta.json()
}
