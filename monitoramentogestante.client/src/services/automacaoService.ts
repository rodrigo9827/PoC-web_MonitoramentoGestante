// Eu sou o cara que fala com a automação do e-Saúde:
//   iniciarBusca -> manda o CNS/CPF da gestante clicada (POST /api/automacao/buscar)
//   lerStatus    -> pergunta em que pé está a busca (GET /api/automacao/status)
// A tela usa os dois: pede a busca e fica perguntando até vir o link (ou um erro).

// O que o servidor responde (os mesmos campos do StatusAutomacao.cs)
export interface StatusAutomacao {
  estado: string
  mensagem: string
  link: string | null
}

export async function iniciarBusca(cns: string, cpf: string): Promise<StatusAutomacao> {
  const resposta = await fetch('/api/automacao/buscar', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({cns, cpf})
  })

  return await resposta.json()
}

export async function lerStatus(): Promise<StatusAutomacao> {
  const resposta = await fetch('/api/automacao/status')
  return await resposta.json()
}
