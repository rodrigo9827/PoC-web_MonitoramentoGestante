<script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import { listarEnfermeiras } from '@/services/enfermeiraService'

  const enfermeiras = ref<string[]>([])
  const enfermeiraSelecionada = ref('')
  const erro = ref('')

  async function carregarEnfermeiras() {
    try {
      enfermeiras.value = await listarEnfermeiras()
    } catch {
      erro.value = 'Não foi possível carregar as enfermeiras. Verifique se o servidor está ligado.'
    }

  }
  onMounted(carregarEnfermeiras)
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
    <selsect id="enfermeira" v-model="enfermeiraSelecionada">
      <option value="" disabled>
        Selecione uma Enfermeira
      </option>
      <option v-for="nome in enfermeiras" :key="nome" :value="nome">
      {{ nome }}
      </option>
    </selsect>

    <p v-if="erro" class="erro">
      {{erro}}
    </p>
    <p v-else-if="enfermeiraSelecionada">
    Selecionada: {{ enfermeiraSelecionada }}
    </p>
  </main>
</template>

<style scoped>
  .erro{
    color: #c00;
  }
</style>
