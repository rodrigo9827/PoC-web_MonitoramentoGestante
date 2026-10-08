// Eu sou o cara que leva o formulário da busca ativa até o servidor (POST /api/buscas-ativas)
// e trago a resposta de volta. Não confiro regra nenhuma: quem confere é o servidor.

// O que a tela envia (os mesmos campos do BuscaAtivaEntrada.cs)
export interface BuscaAtivaEntrada {
  enfermeira: string
  gestante: string
  cns: string
  cpf: string
  dataContato: string | null
  ig: string
  dataRetorno: string | null
  altoRisco: boolean | null
  casoCritico: boolean | null
  qualCasoCritico: string
  classificacao: string
  observacoes: string
  horaInicio: string
}

// O que o servidor responde os mesmos campos do (ResultadoGravacao.cs)
export interface ResultadoGravacao {
  erros: string[]
  sucesso: boolean
  chave: string | null
  dataRetorno: string | null
}

export async function registrarBuscaAtiva(dados: BuscaAtivaEntrada): Promise<ResultadoGravacao> {
  const resposta = await fetch('/api/buscas-ativas', {
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
    erros: [`Erro ${resposta.status} ao registrar a busca ativa.`],
    sucesso: false,
    chave: null,
    dataRetorno: null
  }
}
