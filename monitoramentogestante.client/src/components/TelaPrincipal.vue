<!--
  Eu sou o cara que monta a tela principal: deixo escolher a enfermeira, mostro as gestantes dela
  na tabela (com a coluna Busca Ativa) e abro o formulário de Registrar Busca Ativa.
  Quando o formulário avisa que gravou, mostro o aviso verde e recarrego a lista.
-->
<script setup lang="ts">
  import { ref, onMounted, watch } from 'vue'
  import { listarEnfermeiras } from '@/services/enfermeiraService'
  import { listarGestantes, type Gestante } from '@/services/gestanteService'
  import type { ResultadoGravacao } from '@/services/buscaAtivaService'
  import BuscaAtivaForm from './BuscaAtivaForm.vue'

  const enfermeiras = ref<string[]>([])
  const enfermeiraSelecionada = ref('')
  const gestantes = ref<Gestante[]>([])
  const erro = ref('')
  const aviso = ref('')
  const mostrarBuscaAtiva = ref(false)

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

  // Listener do formulário: a busca ativa foi gravada
  async function aoSalvarBuscaAtiva(resultado: ResultadoGravacao) {
    mostrarBuscaAtiva.value = false
    aviso.value = `Busca ativa registrada. Retorno: ${formatarData(resultado.dataRetorno)}`
    if (enfermeiraSelecionada.value) await carregarGestantes()
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

  // toda vez que a enfermeira escolhida mudar, busca as gestantes dela
  watch(enfermeiraSelecionada, carregarGestantes)
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
        <tr v-for="(g, i) in gestantes" :key="i" :class="{ 'alto-risco': g.altoRisco }">
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

  .alto-risco td {
    color: #ff5252;
  }
</style>
