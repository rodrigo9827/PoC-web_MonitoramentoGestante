Este arquivo explica como Visual Studio criou o projeto.

As seguintes ferramentas foram usadas para gerar este projeto:
- create-vite

As etapas a seguir foram usadas para gerar este projeto:
- Crie um projeto do vue com create-vite: `npm init --yes vue@latest "monitoramentogestante.client" -- --eslint  --typescript `.
- Atualize `vite.config.ts` para configurar o proxy e os certificados.
- Adicione `@type/node` para `vite.config.js` digitar.
- Atualize o componente `HelloWorld` para buscar e exibir informações meteorológicas.
- Adicionar `shims-vue.d.ts` para tipos básicos.
- Criar o arquivo de projeto (`monitoramentogestante.client.esproj`).
- Crie `launch.json` para habilitar a depuração.
- Adicionar projeto à solução.
- Atualize o ponto de extremidade do proxy para ser o ponto de extremidade do servidor back-end.
- Adicione o projeto à lista de projetos de inicialização.
- Grave este arquivo.
