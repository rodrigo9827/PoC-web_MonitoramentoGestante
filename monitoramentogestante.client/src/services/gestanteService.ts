export interface Gestante {
  enfermeira: string | null
  cns: string | null
  gestante: string | null
  dataContato: string | null
  igSemanas: number | null
  igDiasResto: number | null
  dataRetorno: string | null
  altoRisco: number | null
  cpf: string | null
}
export async function listarGestantes(enfermeira: string): Promise<Gestante[]> {
  const resposta = await fetch(`/api/gestantes?enfermeira=${encodeURIComponent(enfermeira)}`)
  if (!resposta.ok) {
    throw new Error(`Erro ${resposta.status} ao buscar gestantes`)
  }
  return await resposta.json()
}
