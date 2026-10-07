export interface Gestante {
  chave: string | null
  gestante: string | null
  cns: string | null
  cpf: string | null
  dataContato: string | null
  igSemanas: number | null
  igDias: number | null
  dataRetorno: string | null
  altoRisco: boolean
  buscaAtiva: string | null
}
export async function listarGestantes(enfermeira: string): Promise<Gestante[]> {
  const resposta = await fetch(`/api/gestantes?enfermeira=${encodeURIComponent(enfermeira)}`)
  if (!resposta.ok) {
    throw new Error(`Erro ${resposta.status} ao buscar gestantes`)
  }
  return await resposta.json()
}
