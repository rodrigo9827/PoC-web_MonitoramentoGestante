<!--
  Eu sou o cara que monta a tela principal: deixo escolher a enfermeira, mostro as gestantes dela
  na tabela, deixo selecionar uma gestante (clique na linha mostra as observações; clique no NOME
  também inicia a automação do e-Saúde), abro os formulários de Busca Ativa e de Finalizar
  Atendimento, e mostro o que a automação está fazendo na linha de status.
-->
<script setup lang="ts">
  import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
  import { listarEnfermeiras } from '@/services/enfermeiraService'
  import { listarGestantes, type Gestante } from '@/services/gestanteService'
  import { listarObservacoes, type Observacao } from '@/services/observacaoService'
  import { iniciarBusca, lerStatus } from '@/services/automacaoService'
  import type { ResultadoGravacao } from '@/services/buscaAtivaService'
  import BuscaAtivaForm from './BuscaAtivaForm.vue'
  import FinalizarAtendimentoForm from './FinalizarAtendimentoForm.vue'


  const enfermeiras = ref<string[]>([])
  const enfermeiraSelecionada = ref('')
  const gestantes = ref<Gestante[]>([])
  const erro = ref('')
  const aviso = ref('')
  const mostrarBuscaAtiva = ref(false)
  const mostrarFinalizar = ref(false)

  const selecionada = ref<Gestante | null>(null)
  const observacoes = ref<Observacao[]>([])
  const carregandoObservacoes = ref(false)
  const erroObservacoes = ref('')


  // o que a automação do e-Saúde está fazendo
  const statusAutomacao = ref('')
  const linkFicha = ref<string | null>(null)   // NOVO: o link da ficha, para a enfermeira clicar
  let timerStatus: number | undefined

  async function carregarEnfermeiras() {
    try {
      enfermeiras.value = await listarEnfermeiras()
      erro.value = ''
    } catch {
      erro.value = 'Não foi possível carregar as enfermeiras. Verifique se o servidor está ligado.'
    }
  }

  async function carregarGestantes() {
    try {
      gestantes.value = await listarGestantes(enfermeiraSelecionada.value)
      erro.value = ''
    } catch {
      gestantes.value = []
      erro.value = 'Não foi possível buscar as gestantes. Verifique se o servidor está ligado.'
    }
  }

  async function selecionarGestante(g: Gestante) {
    selecionada.value = g
    await carregarObservacoes()
  }

  async function carregarObservacoes() {
    observacoes.value = []
    erroObservacoes.value = ''
    const chave = selecionada.value?.chave
    if (!chave) return

    carregandoObservacoes.value = true
    try {
      const lista = await listarObservacoes(chave)
      if (selecionada.value?.chave === chave) observacoes.value = lista
    } catch {
      if (selecionada.value?.chave === chave) {
        erroObservacoes.value = 'Não foi possível buscar as observações. Verifique se o servidor está ligado.'
      }
    } finally {
      if (selecionada.value?.chave === chave) carregandoObservacoes.value = false
    }
  }

  // Listener do clique no NOME da gestante: inicia a automação do e-Saúde
  async function iniciarAutomacao(g: Gestante) {
    if (!g.cns && !g.cpf) {
      statusAutomacao.value = 'Essa gestante não tem CNS nem CPF: não dá para buscar no e-Saúde.'
      return
    }
    pararDePerguntar()
    linkFicha.value = null                      // NOVO
    statusAutomacao.value = 'Iniciando a busca no e-Saúde...'
    try {
      const status = await iniciarBusca(g.cns ?? '', g.cpf ?? '')
      statusAutomacao.value = status.mensagem
      // se já começou (ou estava ocupada), passa a perguntar de 1 em 1 segundo
      perguntarStatus()
    } catch {
      statusAutomacao.value = 'Não foi possível falar com o servidor da automação.'
    }
  }

  function perguntarStatus() {
    pararDePerguntar()
    timerStatus = window.setInterval(async () => {
      try {
        const status = await lerStatus()
        statusAutomacao.value = status.mensagem
        if (status.estado === 'pronta' && status.link) {
          linkFicha.value = status.link          
          window.open(status.link, '_blank')     
          pararDePerguntar()
        } else if (status.estado === 'erro' || status.estado === 'ociosa') {
          pararDePerguntar()
        }
      } catch {
        statusAutomacao.value = 'Não foi possível falar com o servidor da automação.'
        pararDePerguntar()
      }
    }, 1000)
  }

  function pararDePerguntar() {
    if (timerStatus !== undefined) {
      clearInterval(timerStatus)
      timerStatus = undefined
    }
  }

  function limparSelecao() {
    selecionada.value = null
    observacoes.value = []
    erroObservacoes.value = ''
    carregandoObservacoes.value = false
  }

  async function aposGravar(resultado: ResultadoGravacao, textoAviso: string) {
    aviso.value = textoAviso
    if (enfermeiraSelecionada.value) await carregarGestantes()
    selecionada.value = gestantes.value.find(g => g.chave === resultado.chave) ?? null
    await carregarObservacoes()
  }

  async function aoSalvarBuscaAtiva(resultado: ResultadoGravacao) {
    mostrarBuscaAtiva.value = false
    await aposGravar(resultado, `Busca ativa registrada. Retorno: ${formatarData(resultado.dataRetorno)}`)
  }

  async function aoSalvarFinalizar(resultado: ResultadoGravacao) {
    mostrarFinalizar.value = false
    const texto = resultado.dataRetorno
      ? `Atendimento registrado. Retorno: ${formatarData(resultado.dataRetorno)}`
      : 'Atendimento registrado.'
    await aposGravar(resultado, texto)
  }

  function abrirBuscaAtiva() {
    aviso.value = ''
    mostrarFinalizar.value = false
    mostrarBuscaAtiva.value = true
  }

  function abrirFinalizar() {
    aviso.value = ''
    mostrarBuscaAtiva.value = false
    mostrarFinalizar.value = true
  }

  function formatarData(data: string | null): string {
    if (!data) return '—'
    const [ano, mes, dia] = data.split('-')
    return `${dia}/${mes}/${ano}`
  }

  function formatarDataHora(dataHora: string): string {
    const [data, hora] = dataHora.split('T')
    return `${formatarData(data ?? null)} ${(hora ?? '').slice(0, 5)}`
  }

  function formatarIg(g: Gestante): string {
    if (g.igSemanas === null) return '—'
    const dois = (n: number | null) => String(n ?? 0).padStart(2, '0')
    return `${dois(g.igSemanas)}s ${dois(g.igDias)}d`
  }

  function textoOuTraco(valor: string | null): string {
    return valor ? valor : '—'
  }

  onMounted(carregarEnfermeiras)
  onBeforeUnmount(pararDePerguntar)   // ao sair da tela, para de perguntar o status

  watch(enfermeiraSelecionada, () => {
    limparSelecao()
    mostrarFinalizar.value = false
    carregarGestantes()
  })
</script>

<template>
  <main>
    <h1>
      Atendimento de Gestantes
    </h1>
    <!--referenciar com id do banco real quando finalizar tudo-->
    <label for="enfermeira">
      Enfermeiro(a):
    </label>
    <select id="enfermeira" v-model="enfermeiraSelecionada">
      <option value="" disabled>
        Selecione um(a) Enfermeira(o)
      </option>
      <option v-for="nome in enfermeiras" :key="nome" :value="nome">
        {{ nome }}
      </option>
    </select>

    <table v-if="enfermeiraSelecionada">
      <thead>
        <tr>
          <th>Gestante</th>
          <th>CNS</th>
          <th>Contato</th>
          <th>IG</th>
          <th>Retorno</th>
          <th>Risco</th>
          <th>Busca Ativa</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(g, i) in gestantes" :key="i"
            :class="{ 'alto-risco': g.altoRisco, selecionada: g === selecionada }"
            @click="selecionarGestante(g)">
          <!-- clicar no NOME seleciona (o clique sobe para a linha) E inicia a automação -->
          <td class="nome" @click.stop="selecionarGestante(g); iniciarAutomacao(g)"
              title="Clique no nome para abrir a gestante no e-Saúde">
            {{ textoOuTraco(g.gestante) }}
          </td>
          <td>{{ textoOuTraco(g.cns) }}</td>
          <td>{{ formatarData(g.dataContato) }}</td>
          <td>{{ formatarIg(g) }}</td>
          <td>{{ formatarData(g.dataRetorno) }}</td>
          <td>{{ g.altoRisco ? 'Alto Risco' : '' }}</td>
          <td>{{ g.buscaAtiva ?? '' }}</td>
        </tr>
      </tbody>
    </table>

    <p v-if="enfermeiraSelecionada">
      {{ gestantes.length }} gestante(s)
    </p>

    <p v-if="statusAutomacao" class="automacao">
      {{ statusAutomacao }}
      <a v-if="linkFicha" :href="linkFicha" target="_blank" rel="noopener">Abrir a ficha no e-Saúde</a>
    </p>

    <div class="acoes">
      <button type="button" :disabled="mostrarBuscaAtiva" @click="abrirBuscaAtiva">
        Registrar Busca Ativa
      </button>
      <button type="button" :disabled="mostrarFinalizar || !selecionada" @click="abrirFinalizar">
        Finalizar Atendimento
      </button>
    </div>
    <p v-if="!selecionada" class="dica">
      Selecione uma gestante na tabela para finalizar o atendimento dela.
    </p>

    <BuscaAtivaForm v-if="mostrarBuscaAtiva"
                    :enfermeiras="enfermeiras"
                    :enfermeira-inicial="enfermeiraSelecionada"
                    @salvo="aoSalvarBuscaAtiva"
                    @fechar="mostrarBuscaAtiva = false" />

    <FinalizarAtendimentoForm v-if="mostrarFinalizar && selecionada"
                              :enfermeiras="enfermeiras"
                              :enfermeira-inicial="enfermeiraSelecionada"
                              :gestante-inicial="selecionada.gestante ?? ''"
                              :cns-inicial="selecionada.cns ?? ''"
                              :cpf-inicial="selecionada.cpf ?? ''"
                              @salvo="aoSalvarFinalizar"
                              @fechar="mostrarFinalizar = false" />

    <p v-if="erro" class="erro">
      {{ erro }}
    </p>
    <p v-if="aviso" class="aviso">
      {{ aviso }}
    </p>

    <section class="observacoes">
      <h2>Observações da gestante selecionada</h2>

      <p v-if="!selecionada" class="dica">
        Clique numa gestante da tabela para ver as observações.
      </p>
      <p v-else-if="!selecionada.chave" class="dica">
        Essa gestante não tem CNS nem CPF: não há como achar observações.
      </p>
      <p v-else-if="carregandoObservacoes" class="dica">
        Buscando observações...
      </p>
      <p v-else-if="erroObservacoes" class="erro">
        {{ erroObservacoes }}
      </p>
      <p v-else-if="observacoes.length === 0" class="dica">
        Nenhuma observação registrada para essa gestante.
      </p>
      <template v-else>
        <div v-for="(o, i) in observacoes" :key="i" class="observacao">
          <div class="titulo">
            {{ formatarDataHora(o.dataHora) }} — {{ o.formulario }} — {{ o.enfermeira ?? '—' }}
          </div>
          <div class="texto">{{ o.texto }}</div>
        </div>
      </template>
    </section>
  </main>
</template>

<style scoped>
  .erro {
    color: #c00;
  }

  .aviso {
    color: #4caf50;
  }

  .automacao {
    color: #4aa3df;
  }

  .acoes {
    margin-top: 12px;
  }

    .acoes button + button {
      margin-left: 8px;
    }

  table {
    border-collapse: collapse;
    margin-top: 12px;
  }

  th, td {
    text-align: left;
    padding: 4px 12px;
    border-bottom: 1px solid #555;
  }

  tbody tr {
    cursor: pointer;
  }

  .nome {
    text-decoration: underline;
    cursor: pointer;
  }

  .selecionada td {
    background: rgba(66, 184, 131, 0.25);
  }

  .alto-risco td {
    color: #ff5252;
  }

  .observacoes {
    margin-top: 24px;
    border-top: 1px solid #555;
    padding-top: 8px;
  }

  .dica {
    opacity: 0.7;
  }

  .observacao {
    margin-bottom: 12px;
  }

  .titulo {
    font-weight: bold;
  }

  .texto {
    white-space: pre-line;
  }
</style>
