<!--
  Eu sou o cara que monta a tela principal: deixo escolher a enfermeira, mostro as gestantes dela
  na tabela (com a coluna Busca Ativa), deixo selecionar uma gestante com um clique e mostro
  as observações dela no quadro do fim da página. Também abro o formulário de Registrar Busca Ativa;
  quando ele avisa que gravou, mostro o aviso verde, recarrego a lista e seleciono a gestante salva.
-->
<script setup lang="ts">
  import { ref, onMounted, watch } from 'vue'
  import { listarEnfermeiras } from '@/services/enfermeiraService'
  import { listarGestantes, type Gestante } from '@/services/gestanteService'
  import { listarObservacoes, type Observacao } from '@/services/observacaoService'
  import type { ResultadoGravacao } from '@/services/buscaAtivaService'
  import BuscaAtivaForm from './BuscaAtivaForm.vue'

  const enfermeiras = ref<string[]>([])
  const enfermeiraSelecionada = ref('')
  const gestantes = ref<Gestante[]>([])
  const erro = ref('')
  const aviso = ref('')
  const mostrarBuscaAtiva = ref(false)

  // gestante clicada na tabela e as observações dela
  const selecionada = ref<Gestante | null>(null)
  const observacoes = ref<Observacao[]>([])
  const carregandoObservacoes = ref(false)
  const erroObservacoes = ref('')

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

  // Listener do clique na linha da tabela
  async function selecionarGestante(g: Gestante) {
    selecionada.value = g
    await carregarObservacoes()
  }

  async function carregarObservacoes() {
    observacoes.value = []
    erroObservacoes.value = ''
    const chave = selecionada.value?.chave
    if (!chave) return                    // sem CNS e sem CPF: o quadro avisa

    carregandoObservacoes.value = true
    try {
      const lista = await listarObservacoes(chave)
      // se ela clicou em outra gestante enquanto esperava, esta resposta já não vale
      if (selecionada.value?.chave === chave) observacoes.value = lista
    } catch {
      if (selecionada.value?.chave === chave) {
        erroObservacoes.value = 'Não foi possível buscar as observações. Verifique se o servidor está ligado.'
      }
    } finally {
      if (selecionada.value?.chave === chave) carregandoObservacoes.value = false
    }
  }

  function limparSelecao() {
    selecionada.value = null
    observacoes.value = []
    erroObservacoes.value = ''
    carregandoObservacoes.value = false
  }

  // Listener do formulário: a busca ativa foi gravada
  async function aoSalvarBuscaAtiva(resultado: ResultadoGravacao) {
    mostrarBuscaAtiva.value = false
    aviso.value = `Busca ativa registrada. Retorno: ${formatarData(resultado.dataRetorno)}`
    if (enfermeiraSelecionada.value) await carregarGestantes()

    // seleciona a gestante que acabou de ser salva (se ela está na lista desta enfermeira)
    selecionada.value = gestantes.value.find(g => g.chave === resultado.chave) ?? null
    await carregarObservacoes()
  }

  function abrirBuscaAtiva() {
    aviso.value = ''
    mostrarBuscaAtiva.value = true
  }

  // "2026-09-23" -> "23/09/2026" (ou — se vier vazio)
  function formatarData(data: string | null): string {
    if (!data) return '—'
    const [ano, mes, dia] = data.split('-')
    return `${dia}/${mes}/${ano}`
  }

  // "2026-10-08T15:10:00" -> "08/10/2026 15:10"
  function formatarDataHora(dataHora: string): string {
    const [data, hora] = dataHora.split('T')
    return `${formatarData(data ?? null)} ${(hora ?? '').slice(0, 5)}`
  }

  // 30 e 2 -> "30s 02d" (ou — se não houver IG)
  function formatarIg(g: Gestante): string {
    if (g.igSemanas === null) return '—'
    const dois = (n: number | null) => String(n ?? 0).padStart(2, '0')
    return `${dois(g.igSemanas)}s ${dois(g.igDias)}d`
  }

  function textoOuTraco(valor: string | null): string {
    return valor ? valor : '—'
  }

  onMounted(carregarEnfermeiras)

  // toda vez que a enfermeira escolhida mudar: limpa a seleção e busca as gestantes dela
  watch(enfermeiraSelecionada, () => {
    limparSelecao()
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
          <td>{{ textoOuTraco(g.gestante) }}</td>
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

    <button type="button" class="acao" :disabled="mostrarBuscaAtiva" @click="abrirBuscaAtiva">
      Registrar Busca Ativa
    </button>

    <!-- o formulário abre logo abaixo do botão, onde a enfermeira clicou -->
    <BuscaAtivaForm v-if="mostrarBuscaAtiva"
                    :enfermeiras="enfermeiras"
                    :enfermeira-inicial="enfermeiraSelecionada"
                    @salvo="aoSalvarBuscaAtiva"
                    @fechar="mostrarBuscaAtiva = false" />

    <p v-if="erro" class="erro">
      {{ erro }}
    </p>
    <p v-if="aviso" class="aviso">
      {{ aviso }}
    </p>

    <!-- quadro de observações da gestante selecionada (no fim da tela, como na PoC) -->
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

  .acao {
    margin-top: 12px;
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
