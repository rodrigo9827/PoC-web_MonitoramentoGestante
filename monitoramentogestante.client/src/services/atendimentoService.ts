// Eu sou o cara que leva o formulário "Finalizar Atendimento" até o servidor (POST /api/atendimentos)
// e trago a resposta de volta. Não confiro regra nenhuma: quem confere é o servidor.

// a resposta é a mesma da Busca Ativa: aproveito o tipo que já existe
import type { ResultadoGravacao } from './buscaAtivaService'
export type { ResultadoGravacao }

// O que a tela envia (os mesmos 30 campos do FinalizarAtendimentoEntrada.cs)
export interface FinalizarAtendimentoEntrada {
  // sempre aparecem
  enfermeira: string
  teleoperadorId: number | null                     // Id da app.Teleoperadores
  conseguiuContato: boolean | null
  gestante: string
  cns: string
  cpf: string
  numero: string
  dataContato: string | null

  // não conseguiu o contato
  motivoSemContato: string

  // conseguiu o contato
  classificacao: string
  tratamentoSifilis: boolean | null

  // classificação: Gestante
  gestanteRisco: boolean | null
  preNatal: boolean | null
  ig: string
  dataRetorno: string | null

  // classificação: Puérpera
  nasceuVivo: boolean | null
  estabelecimentoIdHospitalParto: number | null     // hospital (Core.Estabelecimento)
  parametroIdOutroLocalDeParto: number | null       // outro local (Core.Parametro)
  outroLocal: string                                // complemento do outro local
  bebeRisco: boolean | null
  obitoNeonatal: boolean | null
  obitoMaterno: boolean | null
  houveViolencia: boolean | null
  tipoViolencia: string
  tipoParto: string
  riscoGestacional: boolean | null
  qualRisco: string

  // sempre aparecem (fim do formulário)
  casoCritico: boolean | null
  qualCasoCritico: string
  observacoes: string
}

export async function finalizarAtendimento(dados: FinalizarAtendimentoEntrada): Promise<ResultadoGravacao> {
  const resposta = await fetch('/api/atendimentos', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(dados)
  })

  // 200 (gravou), 400 (faltou dado) e 500 (banco falhou) vêm com a lista de erros no corpo
  let corpo: ResultadoGravacao | null = null
  try {
    corpo = await resposta.json()
  } catch {
    corpo = null
  }
  if (corpo && Array.isArray(corpo.erros)) return corpo

  return {
    erros: [`Erro ${resposta.status} ao finalizar o atendimento.`],
    sucesso: false,
    chave: null,
    dataRetorno: null
  }
}
