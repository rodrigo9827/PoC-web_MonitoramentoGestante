<!-- Eu sou o cara que mostra o formulário "Registrar Busca Ativa".
     Guardo o que a enfermeira digita, mando para o buscaAtivaService ao clicar em Enviar
     e mostro os erros que o servidor devolver. Eu não confiro regra nenhuma. -->
<script setup lang="ts">
  import { ref } from 'vue'
  import { registrarBuscaAtiva, type ResultadoGravacao } from '@/services/buscaAtivaService'

  const props = defineProps<{
    enfermeiras: string[]
    enfermeiraInicial: string
  }>()

  const emit = defineEmits<{
    salvo: [resultado: ResultadoGravacao]
    fechar: []
  }>()

  // datas e horas no horário do computador, sem converter para outro fuso
  const dois = (n: number) => String(n).padStart(2, '0')

  function hojeLocal(): string {
    const d = new Date()
    return `${d.getFullYear()}-${dois(d.getMonth() + 1)}-${dois(d.getDate())}`
  }

  function agoraLocal(): string {
    const d = new Date()
    return `${hojeLocal()}T${dois(d.getHours())}:${dois(d.getMinutes())}:${dois(d.getSeconds())}`
  }

  const horaInicio = agoraLocal()          

  const enfermeira = ref(props.enfermeiraInicial)
  const gestante = ref('')
  const cns = ref('')
  const cpf = ref('')
  const dataContato = ref(hojeLocal())     
  const ig = ref('')
  const dataRetorno = ref('')
  const altoRisco = ref<boolean | null>(null)
  const casoCritico = ref<boolean | null>(null)
  const qualCasoCritico = ref('')
  const classificacao = ref('Gestante')
  const observacoes = ref('')

  const erros = ref<string[]>([])
  const enviando = ref(false)

  // Listener do botão "Enviar para Enfermeira"
  async function enviar() {
    enviando.value = true
    erros.value = []
    try {
      const resultado = await registrarBuscaAtiva({
        enfermeira: enfermeira.value,
        gestante: gestante.value,
        cns: cns.value,
        cpf: cpf.value,
        dataContato: dataContato.value || null,
        ig: ig.value,
        dataRetorno: dataRetorno.value || null,
        altoRisco: altoRisco.value,
        casoCritico: casoCritico.value,
        qualCasoCritico: casoCritico.value === true ? qualCasoCritico.value : '',
        classificacao: classificacao.value,
        observacoes: observacoes.value,
        horaInicio
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
  <form class="busca-ativa" @submit.prevent="enviar">
    <h2>Registrar Busca Ativa</h2>

    <label for="ba-enfermeira">Enfermeiro(a):</label>
    <select id="ba-enfermeira" v-model="enfermeira">
      <option value="" disabled>Selecione um(a) Enfermeira(o)</option>
      <option v-for="nome in enfermeiras" :key="nome" :value="nome">{{ nome }}</option>
    </select>

    <label for="ba-gestante">Nome da Gestante:</label>
    <input id="ba-gestante" v-model="gestante" type="text" maxlength="255" />

    <label for="ba-cns">CNS da Gestante (15 números):</label>
    <input id="ba-cns" v-model="cns" type="text" inputmode="numeric" maxlength="15" />

    <label for="ba-cpf">CPF da Gestante (11 números):</label>
    <input id="ba-cpf" v-model="cpf" type="text" inputmode="numeric" maxlength="14" />

    <label for="ba-contato">Data do Contato:</label>
    <input id="ba-contato" v-model="dataContato" type="date" />

    <label for="ba-ig">Idade Gestacional (ss+dd, ex.: 30+2):</label>
    <input id="ba-ig" v-model="ig" type="text" maxlength="5" />

    <label for="ba-retorno">Data de retorno (opcional):</label>
    <div>
      <input id="ba-retorno" v-model="dataRetorno" type="date" />
      <small>Se ficar vazia, é calculada pela regra da IG.</small>
    </div>

    <span>Paciente de Alto Risco?</span>
    <div>
      <label><input v-model="altoRisco" type="radio" :value="true" /> Sim</label>
      <label><input v-model="altoRisco" type="radio" :value="false" /> Não</label>
    </div>
    <!-- !alterar para o que o pedro já fez! -->
    <span>Caso crítico?</span>
    <div>
      <label><input v-model="casoCritico" type="radio" :value="true" /> Sim</label>
      <label><input v-model="casoCritico" type="radio" :value="false" /> Não</label>
    </div>

    <template v-if="casoCritico === true">
      <label for="ba-qual">Qual o caso crítico?</label>
      <input id="ba-qual" v-model="qualCasoCritico" type="text" maxlength="1000" />
    </template>

    <span>Classificação da gestante:</span>
    <div>
      <label><input v-model="classificacao" type="radio" value="Gestante" /> Gestante</label>
      <label><input v-model="classificacao" type="radio" value="Puérpera" /> Puérpera</label>
      <label><input v-model="classificacao" type="radio" value="Aborto" /> Aborto</label>
    </div>

    <label for="ba-obs">Observações:</label>
    <textarea id="ba-obs" v-model="observacoes" rows="4"></textarea>

    <ul v-if="erros.length" class="erros">
      <li v-for="e in erros" :key="e">{{ e }}</li>
    </ul>

    <div class="botoes">
      <button type="submit" :disabled="enviando">
        {{ enviando ? 'Enviando...' : 'Enviar para Enfermeira' }}
      </button>
      <button type="button" :disabled="enviando" @click="emit('fechar')">Cancelar</button>
    </div>
  </form>
</template>

<style scoped>
  .busca-ativa {
    display: grid;
    grid-template-columns: max-content 1fr;
    gap: 8px 12px;
    align-items: center;
    margin: 16px 0;
    padding: 12px;
    border: 1px solid #555;
  }

  h2, .erros, .botoes {
    grid-column: 1 / -1;
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

  .botoes button + button {
    margin-left: 8px;
  }
</style>
