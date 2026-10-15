// Eu sou o cara que busca no servidor as duas listas do local do parto:
// os hospitais da cidade de São Paulo (GET /api/hospitais) e os outros locais,
// como fora do município ou parto domiciliar (GET /api/outros-locais-parto)

import type { OpcaoLista } from '@/listas'

export async function listarHospitais(): Promise<OpcaoLista[]> {
  const resposta = await fetch('/api/hospitais')
  if (!resposta.ok) throw new Error('Falha ao buscar os hospitais')
  return await resposta.json()
}

export async function listarOutrosLocaisParto(): Promise<OpcaoLista[]> {
  const resposta = await fetch('/api/outros-locais-parto')
  if (!resposta.ok) throw new Error('Falha ao buscar os outros locais de parto')
  return await resposta.json()
}
