// Eu sou o cara que busca no servidor a lista de teleoperadores ativos
// (GET /api/teleoperadores) e devolve id e nome de cada um para a tela

import type { OpcaoLista } from '@/listas'

export async function listarTeleoperadores(): Promise<OpcaoLista[]> {
  const resposta = await fetch('/api/teleoperadores')
  if (!resposta.ok) throw new Error('Falha ao buscar os teleoperadores')
  return await resposta.json()
}
