// Eu sou o cara que guarda as listas fixas dos formulários (as mesmas do forms.py da PoC):
// locais de parto, motivos de não ter conseguido contato, tipos de violência e de parto.
// Os teleoperadores NÃO ficam aqui: vêm do banco (GET /api/teleoperadores).

export interface OpcaoLista {
  id: number
  nome: string
}

export const MOTIVOS_SEM_CONTATO = [
  'Não existe',
  'Caixa postal',
  'Não Atende',
  'Desconhece',
  'Sem Número',
  'Chamada não completada'
]

export const TIPOS_VIOLENCIA = ['Obstétrica', 'Doméstica', 'Verbal', 'Médica', 'Psicológica', 'Física', 'Sexual']

export const TIPOS_PARTO = ['Vaginal', 'Cesárea', 'Forceps', 'Domiciliar']
