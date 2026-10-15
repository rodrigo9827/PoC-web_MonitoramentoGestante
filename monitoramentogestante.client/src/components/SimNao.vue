<!-- Eu sou o cara que mostra uma pergunta Sim/Não com dois botões de opção.
     Quem me usa liga um ref<boolean | null> com v-model:
     true = Sim, false = Não, null = ainda sem resposta. -->
<script setup lang="ts">
  const resposta = defineModel<boolean | null>({ default: null })
</script>

<template>
  <div class="sim-nao">
    <label><input v-model="resposta" type="radio" :value="true" /> Sim</label>
    <label><input v-model="resposta" type="radio" :value="false" /> Não</label>
  </div>
</template>

<style scoped>
  label + label {
    margin-left: 12px;
  }
</style>
