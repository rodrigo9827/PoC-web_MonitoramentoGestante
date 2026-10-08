// Eu sou o cara que pede ao servidor as observações de uma gestante
// (GET /api/observacoes?chave=) e devolvo a lista, da mais nova para a mais antiga

// O que servidor responde (os mesmos campos de Observacoes.cs)
export interface Observacao {
  dataHora: string
  formulario: string
  enfermeira: string | null
  texto: string
}

export async function listarObservacoes(chave: string): Promise<Observacao[]> {
  const resposta = await fetch(`/api/observacoes?chave=${encodeURIComponent(chave)}`)
  if (!resposta.ok) {
    throw new Error(`[-] - Erro ${resposta.status} ao buscar as observações.`)
  }
  return await resposta.json()
} 
