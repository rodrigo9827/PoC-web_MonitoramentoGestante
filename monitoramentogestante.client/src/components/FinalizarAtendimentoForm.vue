<!-- Eu sou o cara que mostra o formulário "Finalizar Atendimento".
     Já abro com a enfermeira e a gestante selecionada (nome, CNS e CPF), busco no servidor
     as listas de teleoperadores, hospitais e outros locais de parto, mostro só os quadros
     que valem (sem contato / Gestante / Puérpera / Aborto), mando tudo para o atendimentoService
     ao clicar em Enviar (com os Ids das listas) e mostro os erros que o servidor devolver.
     Eu não confiro regra nenhuma. -->
<script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import { finalizarAtendimento, type ResultadoGravacao } from '@/services/atendimentoService'
  import { listarTeleoperadores } from '@/services/teleoperadorService'
  import { listarHospitais, listarOutrosLocaisParto } from '@/services/locaisPartoService'
  import { MOTIVOS_SEM_CONTATO, TIPOS_VIOLENCIA, TIPOS_PARTO, type OpcaoLista } from '@/listas'
  import SimNao from './SimNao.vue'

  const props = defineProps<{
    enfermeiras: string[]
    enfermeiraInicial: string
    gestanteInicial: string
    cnsInicial: string
    cpfInicial: string
  }>()

  const emit = defineEmits<{
    salvo: [resultado: ResultadoGravacao]
    fechar: []
  }>()

  // data de hoje no horário do computador, sem converter para outro fuso
  const dois = (n: number) => String(n).padStart(2, '0')
  function hojeLocal(): string {
    const d = new Date()
    return `${d.getFullYear()}-${dois(d.getMonth() + 1)}-${dois(d.getDate())}`
  }

  // ---------- sempre aparecem (já vêm com a gestante selecionada) ----------
  const enfermeira = ref(props.enfermeiraInicial)
  const teleoperadorId = ref<number | null>(null)
  const conseguiuContato = ref<boolean | null>(null)
  const gestante = ref(props.gestanteInicial)
  const cns = ref(props.cnsInicial)
  const cpf = ref(props.cpfInicial)
  const numero = ref('')
  const dataContato = ref(hojeLocal())

  // ---------- não conseguiu o contato ----------
  const motivoSemContato = ref('')

  // ---------- conseguiu o contato ----------
  const classificacao = ref('')
  const tratamentoSifilis = ref<boolean | null>(null)

  // ---------- classificação: Gestante ----------
  const gestanteRisco = ref<boolean | null>(null)
  const preNatal = ref<boolean | null>(null)
  const ig = ref('')
  const dataRetorno = ref('')

  // ---------- classificação: Puérpera ----------
  const nasceuVivo = ref<boolean | null>(null)
  const tipoLocal = ref<'' | 'hospital' | 'outro'>('')   // onde foi o parto
  const hospitalId = ref<number | null>(null)              // Core.Estabelecimento
  const outroLocalId = ref<number | null>(null)            // Core.Parametro
  const outroLocal = ref('')                               // complemento do outro local
  const bebeRisco = ref<boolean | null>(null)
  const obitoNeonatal = ref<boolean | null>(null)
  const obitoMaterno = ref<boolean | null>(null)
  const houveViolencia = ref<boolean | null>(null)
  const tipoViolencia = ref('')
  const tipoParto = ref('')
  const riscoGestacional = ref<boolean | null>(null)
  const qualRisco = ref('')

  // ---------- sempre aparecem (fim do formulário) ----------
  const casoCritico = ref<boolean | null>(null)
  const qualCasoCritico = ref('')
  const observacoes = ref('')

  // ---------- listas que vêm do banco ----------
  const teleoperadores = ref<OpcaoLista[]>([])
  const hospitais = ref<OpcaoLista[]>([])
  const outrosLocais = ref<OpcaoLista[]>([])
  const avisoListas = ref('')

  onMounted(async () => {
    // as três ao mesmo tempo; se uma falhar, as outras continuam
    const [tele, hosp, outros] = await Promise.allSettled([
      listarTeleoperadores(),
      listarHospitais(),
      listarOutrosLocaisParto()
    ])
    if (tele.status === 'fulfilled') teleoperadores.value = tele.value
    if (hosp.status === 'fulfilled') hospitais.value = hosp.value
    if (outros.status === 'fulfilled') outrosLocais.value = outros.value

    if ([tele, hosp, outros].some(r => r.status === 'rejected'))
      avisoListas.value = 'Algumas listas não carregaram. Atualize a página (F5) antes de enviar.'
  })

  const erros = ref<string[]>([])
  const enviando = ref(false)

  // Listener do botão "Enviar"
  // (mando tudo; o servidor só aproveita os campos da situação marcada)
  async function enviar() {
    enviando.value = true
    erros.value = []
    try {
      const resultado = await finalizarAtendimento({
        enfermeira: enfermeira.value,
        teleoperadorId: teleoperadorId.value,
        conseguiuContato: conseguiuContato.value,
        gestante: gestante.value,
        cns: cns.value,
        cpf: cpf.value,
        numero: numero.value,
        dataContato: dataContato.value || null,

        motivoSemContato: motivoSemContato.value,

        classificacao: classificacao.value,
        tratamentoSifilis: tratamentoSifilis.value,

        gestanteRisco: gestanteRisco.value,
        preNatal: preNatal.value,
        ig: ig.value,
        dataRetorno: dataRetorno.value || null,

        nasceuVivo: nasceuVivo.value,
        // só vai o Id do tipo de local escolhido; o outro vai null
        estabelecimentoIdHospitalParto: tipoLocal.value === 'hospital' ? hospitalId.value : null,
        parametroIdOutroLocalDeParto: tipoLocal.value === 'outro' ? outroLocalId.value : null,
        outroLocal: tipoLocal.value === 'outro' ? outroLocal.value : '',
        bebeRisco: bebeRisco.value,
        obitoNeonatal: obitoNeonatal.value,
        obitoMaterno: obitoMaterno.value,
        houveViolencia: houveViolencia.value,
        tipoViolencia: tipoViolencia.value,
        tipoParto: tipoParto.value,
        riscoGestacional: riscoGestacional.value,
        qualRisco: qualRisco.value,

        casoCritico: casoCritico.value,
        qualCasoCritico: casoCritico.value === true ? qualCasoCritico.value : '',
        observacoes: observacoes.value
      })

      if (resultado.sucesso) {
        emit('salvo', resultado)
      } else {
        erros.value = resultado.erros
      }
    } catch {
      erros.value = ['Não foi possível falar com o servidor. Verifique se ele está ligado.']
    } finally {
      enviando.value = false
    }
  }
</script>

<template>
  <form class="finalizar" @submit.prevent="enviar">
    <h2>Finalizar Atendimento</h2>

    <p v-if="avisoListas" class="aviso">{{ avisoListas }}</p>

    <!-- ================= SEMPRE APARECEM ================= -->
    <label for="fa-enfermeira">Enfermeiro(a):</label>
    <select id="fa-enfermeira" v-model="enfermeira">
      <option value="" disabled>Selecione um(a) Enfermeira(o)</option>
      <option v-for="nome in enfermeiras" :key="nome" :value="nome">{{ nome }}</option>
    </select>

    <label for="fa-teleoperador">Teleoperador(a):</label>
    <select id="fa-teleoperador" v-model="teleoperadorId">
      <option :value="null" disabled>Escolha o teleoperador (ou Nenhum)</option>
      <option v-for="t in teleoperadores" :key="t.id" :value="t.id">{{ t.nome }}</option>
    </select>

    <span>Conseguiu o contato?</span>
    <SimNao v-model="conseguiuContato" />

    <label for="fa-gestante">Nome da Gestante:</label>
    <input id="fa-gestante" v-model="gestante" type="text" maxlength="255" />

    <label for="fa-cns">CNS da Gestante (15 números):</label>
    <input id="fa-cns" v-model="cns" type="text" inputmode="numeric" maxlength="15" />

    <label for="fa-cpf">CPF da Gestante (11 números):</label>
    <input id="fa-cpf" v-model="cpf" type="text" inputmode="numeric" maxlength="14" />

    <label for="fa-numero">Número do contato:</label>
    <input id="fa-numero" v-model="numero" type="text" inputmode="tel" maxlength="50" />

    <label for="fa-contato">Data do Contato:</label>
    <input id="fa-contato" v-model="dataContato" type="date" />

    <!-- ================= NÃO CONSEGUIU O CONTATO ================= -->
    <template v-if="conseguiuContato === false">
      <h3 class="secao">Sem contato</h3>

      <label for="fa-motivo">Qual o motivo?</label>
      <select id="fa-motivo" v-model="motivoSemContato">
        <option value="" disabled>Escolha o motivo</option>
        <option v-for="m in MOTIVOS_SEM_CONTATO" :key="m" :value="m">{{ m }}</option>
      </select>
    </template>

    <!-- ================= CONSEGUIU O CONTATO ================= -->
    <template v-if="conseguiuContato === true">
      <h3 class="secao">Contato realizado</h3>

      <span>Classificação:</span>
      <div>
        <label><input v-model="classificacao" type="radio" value="Gestante" /> Gestante</label>
        <label><input v-model="classificacao" type="radio" value="Puérpera" /> Puérpera</label>
        <label><input v-model="classificacao" type="radio" value="Aborto" /> Aborto</label>
      </div>

      <!-- ---------- Gestante ---------- -->
      <template v-if="classificacao === 'Gestante'">
        <span>É gestante de risco?</span>
        <SimNao v-model="gestanteRisco" />

        <span>Acompanhamento pré-natal?</span>
        <SimNao v-model="preNatal" />

        <label for="fa-ig">Idade Gestacional (ss+dd, ex.: 30+2):</label>
        <input id="fa-ig" v-model="ig" type="text" maxlength="5" />

        <label for="fa-retorno">Data de retorno (opcional):</label>
        <div>
          <input id="fa-retorno" v-model="dataRetorno" type="date" />
          <small>Se informar a IG e deixar vazia, é calculada pela regra da IG.</small>
        </div>
      </template>

      <!-- ---------- Puérpera ---------- -->
      <template v-if="classificacao === 'Puérpera'">
        <span>Nasceu vivo?</span>
        <SimNao v-model="nasceuVivo" />

        <span>O parto foi em:</span>
        <div>
          <label><input v-model="tipoLocal" type="radio" value="hospital" /> Hospital</label>
          <label><input v-model="tipoLocal" type="radio" value="outro" /> Outro local</label>
        </div>

        <template v-if="tipoLocal === 'hospital'">
          <label for="fa-hospital">Hospital do parto:</label>
          <select id="fa-hospital" v-model="hospitalId">
            <option :value="null" disabled>Escolha o hospital</option>
            <option v-for="h in hospitais" :key="h.id" :value="h.id">{{ h.nome }}</option>
          </select>
        </template>

        <template v-if="tipoLocal === 'outro'">
          <label for="fa-outro-local-id">Qual outro local?</label>
          <select id="fa-outro-local-id" v-model="outroLocalId">
            <option :value="null" disabled>Escolha o local</option>
            <option v-for="o in outrosLocais" :key="o.id" :value="o.id">{{ o.nome }}</option>
          </select>

          <label for="fa-outro-local">Complemento (opcional):</label>
          <input id="fa-outro-local" v-model="outroLocal" type="text" maxlength="500"
                 placeholder="Ex.: nome da cidade ou da maternidade" />
        </template>

        <span>É um bebê de risco?</span>
        <SimNao v-model="bebeRisco" />

        <span>Óbito neonatal?</span>
        <SimNao v-model="obitoNeonatal" />

        <span>Houve óbito materno?</span>
        <SimNao v-model="obitoMaterno" />

        <span>Houve violência?</span>
        <SimNao v-model="houveViolencia" />

        <template v-if="houveViolencia === true">
          <label for="fa-violencia">Qual violência?</label>
          <select id="fa-violencia" v-model="tipoViolencia">
            <option value="" disabled>Escolha o tipo</option>
            <option v-for="v in TIPOS_VIOLENCIA" :key="v" :value="v">{{ v }}</option>
          </select>
        </template>

        <label for="fa-parto">Tipo de parto:</label>
        <select id="fa-parto" v-model="tipoParto">
          <option value="" disabled>Escolha o tipo</option>
          <option v-for="p in TIPOS_PARTO" :key="p" :value="p">{{ p }}</option>
        </select>

        <span>Teve risco gestacional?</span>
        <SimNao v-model="riscoGestacional" />

        <template v-if="riscoGestacional === true">
          <label for="fa-qual-risco">Qual?</label>
          <input id="fa-qual-risco" v-model="qualRisco" type="text" maxlength="500" />
        </template>
      </template>

      <!-- ---------- comum às três classificações (Aborto não tem mais nada) ---------- -->
      <span>Tratamento de sífilis?</span>
      <SimNao v-model="tratamentoSifilis" />
    </template>

    <!-- ================= SEMPRE APARECEM (FIM) ================= -->
    <h3 class="secao">Para todos</h3>

    <!-- TODO: alterar o caso crítico para o que o Pedro já fez -->
    <span>Caso crítico?</span>
    <SimNao v-model="casoCritico" />

    <template v-if="casoCritico === true">
      <label for="fa-qual-caso">Qual o caso crítico?</label>
      <input id="fa-qual-caso" v-model="qualCasoCritico" type="text" maxlength="1000" />
    </template>

    <label for="fa-obs">Observações:</label>
    <textarea id="fa-obs" v-model="observacoes" rows="4"></textarea>

    <ul v-if="erros.length" class="erros">
      <li v-for="e in erros" :key="e">{{ e }}</li>
    </ul>

    <div class="botoes">
      <button type="submit" :disabled="enviando">
        {{ enviando ? 'Enviando...' : 'Enviar' }}
      </button>
      <button type="button" :disabled="enviando" @click="emit('fechar')">Cancelar</button>
    </div>
  </form>
</template>

<style scoped>
  .finalizar {
    display: grid;
    grid-template-columns: max-content 1fr;
    gap: 8px 12px;
    align-items: center;
    margin: 16px 0;
    padding: 12px;
    border: 1px solid #555;
  }

  h2, .secao, .erros, .botoes, .aviso {
    grid-column: 1 / -1;
  }

  .secao {
    margin: 8px 0 0;
    font-size: 1em;
    opacity: 0.8;
    border-top: 1px solid #555;
    padding-top: 8px;
  }

  input[type='text'], select, textarea {
    width: 100%;
    max-width: 420px;
  }

  small {
    margin-left: 8px;
    opacity: 0.7;
  }

  label + label {
    margin-left: 12px;
  }

  .erros {
    color: #ff5252;
    margin: 0;
    padding-left: 20px;
  }

  .aviso {
    color: #ffb74d;
    margin: 0;
  }

  .botoes button + button {
    margin-left: 8px;
  }
</style>
